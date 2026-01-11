// SPDX-FileCopyrightText: 2025 AirFryerBuyOneGetOneFree <jakoblondon01@gmail.com>
// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Shared.Administration.Logs;
using Content.Shared._Impstation.CrewMedal;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Interaction;
using System.Linq;
using System.Text;

namespace Content.Server._Impstation.CrewMedal;

public sealed class CrewMedalSystem : SharedCrewMedalSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrewMedalComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CrewMedalComponent, CrewMedalReasonChangedMessage>(OnReasonChanged);
        SubscribeLocalEvent<CrewMedalComponent, CrewMedalAwardDoAfterEvent>(OnDoAfterAward);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnAfterInteract(EntityUid uid, CrewMedalComponent component, AfterInteractEvent args)
    {
        // Only allow awarding if not already awarded.
        if (component.Awarded)
            return;

        if (args.Target == null || !args.CanReach)
            return;

        var doAfterTime = 2.0f;

        var doAfter = new CrewMedalAwardDoAfterEvent();

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, doAfterTime, doAfter, uid, target: args.Target, used: uid)
        {
            DistanceThreshold = SharedInteractionSystem.InteractionRange,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        });
    }

    private void OnReasonChanged(EntityUid uid, CrewMedalComponent medalComp, CrewMedalReasonChangedMessage args)
    {
        if (medalComp.Awarded)
            return;
        medalComp.Reason = args.Reason[..Math.Min(medalComp.MaxCharacters, args.Reason.Length)];
        Dirty(uid, medalComp);

        // Log medal reason change
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.Actor):user} set {ToPrettyString(uid):entity} to apply the award reason \"{medalComp.Reason}\"");
    }

    private void OnDoAfterAward(EntityUid uid, CrewMedalComponent comp, CrewMedalAwardDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        if (comp.Awarded)
            return;

        var recipient = args.Target.Value;

        comp.Recipient = Identity.Name(recipient, EntityManager);
        comp.Awarded = true;
        Dirty(uid, comp);

        _popup.PopupEntity(Loc.GetString("comp-crew-medal-award-text", ("recipient", comp.Recipient), ("medal", Name(uid))), uid);

        // Try to equip to neck slot if empty, otherwise try to put in any free hand.
        if (!_inventory.TryGetSlotEntity(recipient, "neck", out _))
        {
            // neck slot empty, try to equip
            _inventory.TryEquip(recipient, uid, "neck", silent: true, force: true);
        }
        else
        {
            // neck occupied, try to put in a free hand
            _hands.TryPickupAnyHand(recipient, uid, checkActionBlocker: false);
        }

        // Log medal awarding
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(recipient):player} was awarded the {ToPrettyString(uid):entity} by {ToPrettyString(args.User):user} with the award reason \"{comp.Reason}\"");
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        // medal name, recipient name, reason
        var medals = new List<(string, string, string)>();
        var query = EntityQueryEnumerator<CrewMedalComponent>();
        while (query.MoveNext(out var uid, out var crewMedalComp))
        {
            if (crewMedalComp.Awarded)
                medals.Add((Name(uid), crewMedalComp.Recipient, crewMedalComp.Reason));
        }
        var count = medals.Count;
        if (count == 0)
            return;

        medals.OrderBy(f => f.Item2);
        var result = new StringBuilder();
        result.AppendLine(Loc.GetString("comp-crew-medal-round-end-result", ("count", count)));
        foreach (var medal in medals)
        {
            result.AppendLine(Loc.GetString("comp-crew-medal-round-end-list", ("medal", medal.Item1), ("recipient", medal.Item2), ("reason", medal.Item3)));
        }
        ev.AddLine(result.AppendLine().ToString());
    }
}
