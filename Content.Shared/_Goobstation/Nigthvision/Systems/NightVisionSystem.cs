using Content.Shared.NightVision.Components;
using Content.Shared.Inventory;
using Content.Shared.Actions;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Content.Shared.Clothing;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.NightVision.Systems;

public sealed class NightVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISerializationManager _serMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        if (_net.IsServer)
        {
            SubscribeLocalEvent<NightVisionComponent, ComponentStartup>(OnComponentStartup);
            SubscribeLocalEvent<NightVisionComponent, ComponentShutdown>(OnComponentShutdown);
            SubscribeLocalEvent<NightVisionComponent, ClothingGotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<NightVisionComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
        }
        SubscribeLocalEvent<NightVisionComponent, NVInstantActionEvent>(OnActionToggle);
    }

    [ValidatePrototypeId<EntityPrototype>]
    private const string SwitchNightVisionAction = "SwitchNightVision";

    private void OnComponentStartup(EntityUid uid, NightVisionComponent component, ComponentStartup args)
    {
        if (component.IsToggle)
            _actionsSystem.AddAction(uid, ref component.ActionContainer, SwitchNightVisionAction);
        else
            component.IsNightVision = true;
    }

    private void OnComponentShutdown(EntityUid uid, NightVisionComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ActionContainer);
    }

    private void OnGotEquipped(EntityUid uid, NightVisionComponent component, ref ClothingGotEquippedEvent args)
    {
        if (!HasComp<NightVisionComponent>(args.Wearer))
        {
            var newComp = new NightVisionComponent
            {
                IsNightVision = component.IsNightVision,
                IsToggle = component.IsToggle,
                PlaySoundOn = component.PlaySoundOn,
                OnOffSound = component.OnOffSound
            };
            AddComp(args.Wearer, newComp, true);
            // dirty because yea. They have a fancy new action or overlay now.
            DirtyEntity(args.Wearer);
        }
    }

    private void OnGotUnequipped(EntityUid uid, NightVisionComponent component, ref ClothingGotUnequippedEvent args)
    {
        if (HasComp<NightVisionComponent>(args.Wearer))
        {
            RemComp<NightVisionComponent>(args.Wearer);
            DirtyEntity(args.Wearer);
        }
    }

    private void OnActionToggle(EntityUid uid, NightVisionComponent component, NVInstantActionEvent args)
    {
        component.IsNightVision = !component.IsNightVision;
        var changeEv = new NightVisionChangedEvent(component.IsNightVision); // theres nothing anywhere subscribed to this event? Unless theres metacode/macros involved...
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
        _actionsSystem.SetCooldown(component.ActionContainer, TimeSpan.FromSeconds(1));
        if (component.IsNightVision && component.PlaySoundOn)
        {
            if (_net.IsServer)
                _audioSystem.PlayPvs(component.OnOffSound, uid);
        }
    }

    [PublicAPI]
    public void UpdateIsNightVision(EntityUid uid, NightVisionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        var old = component.IsNightVision;


        var ev = new CanVisionAttemptEvent(); // theres nothing anywhere subscribed to this event?
        RaiseLocalEvent(uid, ev);
        component.IsNightVision = ev.NightVision;

        if (old == component.IsNightVision)
            return;

        var changeEv = new NightVisionChangedEvent(component.IsNightVision);
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
    }
}

public sealed class CanVisionAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public bool NightVision => Cancelled;
    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.MASK | SlotFlags.HEAD;
}
