// SPDX-FileCopyrightText: 2026 ALooseGoose <ALooseGoosey@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;

namespace Content.Shared._Impstation.Replicator;

public sealed class ReplicatorItemActionsSystem : EntitySystem
{

    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorComponent, ReplicatorOmnitoolActionEvent>(OnReplicatorOmnitoolActionEvent);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorOmnitoolDoAfterEvent>(OnReplicatorOmnitoolDoAfter);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorWelderActionEvent>(OnReplicatorWelderActionEvent);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorWelderDoAfterEvent>(OnReplicatorWelderDoAfter);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorArmActionEvent>(OnReplicatorArmActionEvent);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorArmDoAfterEvent>(OnReplicatorArmDoAfter);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorAACActionEvent>(OnReplicatorAACActionEvent);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorAACDoAfterEvent>(OnReplicatorAACDoAfter);
    }

    private void OnReplicatorOmnitoolActionEvent(EntityUid uid, ReplicatorComponent component, ReplicatorOmnitoolActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (hands.ActiveHand?.HeldEntity == null)
            return;

        if (hands.ActiveHand.HeldEntity != null && EntityManager.EntityExists(hands.ActiveHand.HeldEntity.Value))
        {
            _handsSystem.RemoveHands(uid);
        }

        var doAfterEvent = new DoAfterArgs(EntityManager, uid, TimeSpan.FromMicroseconds(1), new ReplicatorOmnitoolDoAfterEvent(), uid, null, null)
        {
            BreakOnDamage = false,
            BreakOnDropItem = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            CancelDuplicate = true,
            Hidden = true,
            NeedHand = false
        };

        _doAfterSystem.TryStartDoAfter(doAfterEvent);
    }

    private void OnReplicatorOmnitoolDoAfter(EntityUid uid, ReplicatorComponent component, ReplicatorOmnitoolDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        _handsSystem.AddHand(uid, "ReplicatorHand", HandLocation.Middle);
        var tool = Spawn("OmnitoolUnremoveable");
        _handsSystem.DoPickup(uid, hands.Hands["ReplicatorHand"], tool);
        EnsureComp<UnremoveableComponent>(tool);
    }

    private void OnReplicatorWelderActionEvent(EntityUid uid, ReplicatorComponent component, ReplicatorWelderActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (hands.ActiveHand?.HeldEntity == null)
            return;

        if (hands.ActiveHand.HeldEntity != null && EntityManager.EntityExists(hands.ActiveHand.HeldEntity.Value))
        {
            _handsSystem.RemoveHands(uid);
        }

        var doAfterEvent = new DoAfterArgs(EntityManager, uid, TimeSpan.FromMicroseconds(1), new ReplicatorWelderDoAfterEvent(), uid, null, null)
        {
            BreakOnDamage = false,
            BreakOnDropItem = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            CancelDuplicate = true,
            Hidden = true,
            NeedHand = false
        };

        _doAfterSystem.TryStartDoAfter(doAfterEvent);
    }

    private void OnReplicatorWelderDoAfter(EntityUid uid, ReplicatorComponent component, ReplicatorWelderDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        _handsSystem.AddHand(uid, "ReplicatorHand", HandLocation.Middle);
        var tool = Spawn("WelderExperimentalUnremoveable");
        _handsSystem.DoPickup(uid, hands.Hands["ReplicatorHand"], tool);
        EnsureComp<UnremoveableComponent>(tool);
    }

    private void OnReplicatorArmActionEvent(EntityUid uid, ReplicatorComponent component, ReplicatorArmActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (hands.ActiveHand?.HeldEntity == null)
            return;

        if (hands.ActiveHand.HeldEntity != null && EntityManager.EntityExists(hands.ActiveHand.HeldEntity.Value))
        {
            _handsSystem.RemoveHands(uid);
        }

        var doAfterEvent = new DoAfterArgs(EntityManager, uid, TimeSpan.FromMicroseconds(1), new ReplicatorArmDoAfterEvent(), uid, null, null)
        {
            BreakOnDamage = false,
            BreakOnDropItem = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            CancelDuplicate = true,
            Hidden = true,
            NeedHand = false
        };

        _doAfterSystem.TryStartDoAfter(doAfterEvent);
    }

    private void OnReplicatorArmDoAfter(EntityUid uid, ReplicatorComponent component, ReplicatorArmDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        _handsSystem.AddHand(uid, "ReplicatorHand", HandLocation.Middle);
        var tool = Spawn("ReplicatorT3Weapon");
        _handsSystem.DoPickup(uid, hands.Hands["ReplicatorHand"], tool);
        EnsureComp<UnremoveableComponent>(tool);
    }

    private void OnReplicatorAACActionEvent(EntityUid uid, ReplicatorComponent component, ReplicatorAACActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (hands.ActiveHand?.HeldEntity == null)
            return;

        if (hands.ActiveHand.HeldEntity != null && EntityManager.EntityExists(hands.ActiveHand.HeldEntity.Value))
        {
            _handsSystem.RemoveHands(uid);
        }

        var doAfterEvent = new DoAfterArgs(EntityManager, uid, TimeSpan.FromMicroseconds(1), new ReplicatorAACDoAfterEvent(), uid, null, null)
        {
            BreakOnDamage = false,
            BreakOnDropItem = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            CancelDuplicate = true,
            Hidden = true,
            NeedHand = false
        };

        _doAfterSystem.TryStartDoAfter(doAfterEvent);
    }

    private void OnReplicatorAACDoAfter(EntityUid uid, ReplicatorComponent component, ReplicatorAACDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        _handsSystem.AddHand(uid, "ReplicatorHand", HandLocation.Middle);
        var tool = Spawn("ReplicatorAAC");
        _handsSystem.DoPickup(uid, hands.Hands["ReplicatorHand"], tool);
        EnsureComp<UnremoveableComponent>(tool);
    }


}
