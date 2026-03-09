using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Magic.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Physics;
using Content.Shared._Funkystation.WizardFamiliar;
using Robust.Shared.Localization;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.WizardFamiliar;

public sealed class WizardFamiliarSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    private const string SummonActionPrototype = "ActionSummonMiniDragon";
    private const string TeleportActionPrototype = "ActionMiniDragonTeleportToWizard";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InstantSpawnSpellEvent>(OnInstantSpawnSpell, before: [typeof(Content.Shared.Magic.SharedMagicSystem)]);
        SubscribeLocalEvent<WizardFamiliarComponent, MobStateChangedEvent>(OnFamiliarDeath);
        SubscribeLocalEvent<WizardFamiliarComponent, MiniDragonTeleportToWizardEvent>(OnTeleportToWizard);
    }

    private void OnInstantSpawnSpell(InstantSpawnSpellEvent args)
    {
        if (args.Handled || args.Prototype != "MobMiniDragonFamiliar")
            return;

        var performer = args.Performer;

        // Block if wizard already has a living familiar
        var query = EntityQueryEnumerator<WizardFamiliarComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Wizard != performer)
                continue;
            if (_mobState.IsDead(uid))
                continue;

            _popup.PopupEntity(Loc.GetString("wizard-familiar-already-summoned"), performer, performer);
            args.Handled = true;
            return;
        }

        var transform = Transform(performer);
        var spawnPos = transform.Coordinates;
        var dragon = Spawn("MobMiniDragonFamiliar", spawnPos.SnapToGrid(EntityManager, _mapManager));

        EnsureComp<PreventCollideComponent>(dragon).Uid = performer;

        if (TryComp<WizardFamiliarComponent>(dragon, out var familiarComp))
        {
            familiarComp.Wizard = performer;
        }

        _faction.IgnoreEntity((dragon, null), (performer, null));

        if (TryComp<HTNComponent>(dragon, out var htn))
            _npc.SetBlackboard(dragon, NPCBlackboard.FollowTarget, new EntityCoordinates(performer, Vector2.Zero), htn);

        _actions.AddAction(dragon, TeleportActionPrototype);

        args.Handled = true;
    }

    private void OnFamiliarDeath(EntityUid uid, WizardFamiliarComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var wizard = component.Wizard;
        if (!wizard.HasValue || !Exists(wizard.Value))
            return;

        if (!TryComp<ActionsComponent>(wizard.Value, out var actions))
            return;

        foreach (var actionId in actions.Actions)
        {
            if (!TryComp(actionId, out MetaDataComponent? meta))
                continue;
            if (meta.EntityPrototype?.ID != SummonActionPrototype)
                continue;

            _actions.SetCooldown(actionId, TimeSpan.FromMinutes(10));
            break;
        }
    }

    private void OnTeleportToWizard(EntityUid uid, WizardFamiliarComponent component, MiniDragonTeleportToWizardEvent args)
    {
        if (args.Handled)
            return;

        var wizard = component.Wizard;
        if (!wizard.HasValue || !Exists(wizard.Value) || Deleted(wizard.Value))
        {
            _popup.PopupEntity(Loc.GetString("wizard-familiar-wizard-gone"), uid, uid);
            args.Handled = true;
            return;
        }

        var wizardXform = Transform(wizard.Value);
        var dragonXform = Transform(uid);
        if (wizardXform.MapID != dragonXform.MapID)
        {
            _popup.PopupEntity(Loc.GetString("wizard-familiar-wizard-gone"), uid, uid);
            args.Handled = true;
            return;
        }

        _transform.SetCoordinates(uid, wizardXform.Coordinates);
        _transform.AttachToGridOrMap(uid, dragonXform);
        args.Handled = true;
    }
}
