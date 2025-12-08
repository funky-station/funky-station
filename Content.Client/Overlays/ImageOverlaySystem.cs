using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates; // <--- ADDED: Needed for State Events
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ImageOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public static readonly ProtoId<ShaderPrototype> ImageShader = "ImageMask";
    private ImageOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new();

        // Lifecycle Events
        SubscribeLocalEvent<ImageOverlayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ImageOverlayComponent, ComponentShutdown>(OnShutdown);

        // Data Sync Event
        SubscribeLocalEvent<ImageOverlayComponent, AfterAutoHandleStateEvent>(OnHandleState);

        // Equipment Events
        SubscribeLocalEvent<ImageOverlayComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ImageOverlayComponent, GotUnequippedEvent>(OnUnequipped);

        // Player Events
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartup(EntityUid uid, ImageOverlayComponent comp, ComponentStartup args)
    {
        RefreshOverlay();
    }

    private void OnShutdown(EntityUid uid, ImageOverlayComponent comp, ComponentShutdown args)
    {
        RefreshOverlay(ignoreComp: comp);
    }

    private void OnHandleState(EntityUid uid, ImageOverlayComponent comp, ref AfterAutoHandleStateEvent args)
    {
        // The component data (path/color) has just been updated from the server.
        // Refresh to ensure the overlay picks up the new values.
        RefreshOverlay();
    }

    private void OnEquipped(EntityUid uid, ImageOverlayComponent comp, GotEquippedEvent args)
    {
        RefreshOverlay();
    }

    private void OnUnequipped(EntityUid uid, ImageOverlayComponent comp, GotUnequippedEvent args)
    {
        RefreshOverlay();
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        RefreshOverlay();
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        _overlay.ImageShaders.Clear();
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void RefreshOverlay(ImageOverlayComponent? ignoreComp = null)
    {
        var player = _playerManager.LocalSession?.AttachedEntity;
        if (player == null) return;

        _overlay.ImageShaders.Clear();
        bool hasOverlay = false;

        var slotsToCheck = new[] { "head", "eyes", "mask" };

        foreach (var slot in slotsToCheck)
        {
            if (_inventorySystem.TryGetSlotEntity(player.Value, slot, out var item))
            {
                if (TryComp<ImageOverlayComponent>(item, out var comp))
                {
                    if (comp == ignoreComp) continue;

                    // If data hasn't arrived yet (null), skip for now.
                    // OnHandleState will catch it milliseconds later.
                    if (comp.PathToOverlayImage == null) continue;

                    var values = new ImageShaderValues
                    {
                        PathToOverlayImage = comp.PathToOverlayImage.Value,
                        AdditionalColorOverlay = comp.AdditionalColorOverlay
                    };

                    _overlay.ImageShaders.Add((_prototypeManager.Index(ImageShader).InstanceUnique(), values));
                    hasOverlay = true;
                }
            }
        }

        if (hasOverlay)
        {
            if (!_overlayMan.HasOverlay<ImageOverlay>())
                _overlayMan.AddOverlay(_overlay);
        }
        else
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
