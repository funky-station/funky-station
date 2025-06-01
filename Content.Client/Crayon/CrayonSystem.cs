using Content.Client.Decals.Overlays;
using Content.Client.Items;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.Crayon;
using Content.Shared.Decals;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Localization;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client.Crayon;

public sealed class CrayonSystem : SharedCrayonSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;


    private CrayonPlacementOverlay _placementOverlay = default!;
    private string? _selectedState;
    private Color _color = Color.Wheat;
    private bool _active;

    // Didn't do in shared because I don't think most of the server stuff can be predicted.
    public override void Initialize()
    {
        base.Initialize();

        if (!_overlayManager.HasOverlay<CrayonPlacementOverlay>())
        {
            _overlayManager.AddOverlay(new CrayonPlacementOverlay(_transform));
        }

        _placementOverlay = _overlayManager.GetOverlay<CrayonPlacementOverlay>();

        SubscribeLocalEvent<CrayonComponent, CrayonSelectMessage>(OnCrayonBoundUI);
        SubscribeLocalEvent<CrayonComponent, ComponentHandleState>(OnCrayonHandleState);
        SubscribeLocalEvent<CrayonComponent, DroppedEvent>(OnCrayonDropped);
        //SubscribeLocalEvent<CrayonComponent, BasePickupAttemptEvent>(OnCrayonPickup);
        Subs.ItemStatus<CrayonComponent>(ent => new StatusControl(ent));
    }

    private void OnCrayonHandleState(EntityUid uid, CrayonComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not CrayonComponentState state) return;

        component.Color = state.Color;
        component.SelectedState = state.State;
        component.Charges = state.Charges;
        component.Capacity = state.Capacity;
        component.Infinite = state.Infinite;

        component.UIUpdateNeeded = true;

        _color = state.Color;
        _selectedState = state.State;

        _placementOverlay.SetActiveDecal(_prototypeManager.Index<DecalPrototype>(state.State), state.Color);
    }

    public (DecalPrototype? Decal, bool Snap, Angle Angle, Color Color) GetActiveDecal()
    {
        return _selectedState != null ?
            (_prototypeManager.Index<DecalPrototype>(_selectedState), false, Angle.Zero, _color) :
            (null, false, Angle.Zero, Color.Wheat);
    }

    private void OnCrayonBoundUI(EntityUid uid, CrayonComponent component, CrayonSelectMessage args)
    {
        // Check if the selected state is valid
        if (!_prototypeManager.TryIndex<DecalPrototype>(args.State, out var prototype) || !prototype.Tags.Contains("crayon"))
            return;

        _selectedState = args.State;
    }

    private void OnCrayonDropped(EntityUid uid, CrayonComponent component, DroppedEvent args)
    {
        _active = false;
    }

    private void OnCrayonPickup(EntityUid uid, CrayonComponent component, DroppedEvent args)
    {
        _active = true;
    }

    private void OnCrayonInit(EntityUid uid, CrayonComponent component, ComponentInit args)
    {
        component.Charges = component.Capacity;

        // Get the first one from the catalog and set it as default
        var decal = _prototypeManager.EnumeratePrototypes<DecalPrototype>().FirstOrDefault(x => x.Tags.Contains("crayon"));
        component.SelectedState = decal?.ID ?? string.Empty;
        Dirty(uid, component);
    }


    private sealed class StatusControl : Control
    {
        private readonly CrayonComponent _parent;
        private readonly RichTextLabel _label;

        public StatusControl(CrayonComponent parent)
        {
            _parent = parent;
            _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
            AddChild(_label);

            parent.UIUpdateNeeded = true;
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            if (!_parent.UIUpdateNeeded)
            {
                return;
            }

            _parent.UIUpdateNeeded = false;
            _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("crayon-drawing-label",
                ("color", _parent.Color),
                ("state", _parent.SelectedState),
                ("charges", _parent.Charges),
                ("capacity", _parent.Capacity),
                ("infinite", _parent.Infinite))); // imp
        }
    }
}
