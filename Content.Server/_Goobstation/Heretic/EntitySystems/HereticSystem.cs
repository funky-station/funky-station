// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Objectives.Components;
using Content.Server.Store.Systems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Store.Components;
using Content.Shared.Heretic.Prototypes;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio;
using Content.Server.Temperature.Components;
using Content.Server.Body.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Damage;
using Content.Server.Heretic.Components;
using Content.Server.Antag;
using Robust.Shared.Random;
using System.Linq;
using Content.Server.AlertLevel;
using Content.Shared.Humanoid;
using Robust.Server.Player;
using Robust.Shared.Timing;
using Content.Server.Revolutionary.Components;
using Content.Server.Station.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly HereticKnowledgeSystem _knowledge = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<HereticComponent, ComponentInit>(OnCompInit);

        SubscribeLocalEvent<HereticComponent, EventHereticUpdateTargets>(OnUpdateTargets);
        SubscribeLocalEvent<HereticComponent, EventHereticRerollTargets>(OnRerollTargets);
        SubscribeLocalEvent<HereticComponent, EventHereticAscension>(OnAscension);

        SubscribeLocalEvent<HereticComponent, BeforeDamageChangedEvent>(OnBeforeDamage);
        SubscribeLocalEvent<HereticComponent, DamageModifyEvent>(OnDamage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<HereticComponent>();

        while (query.MoveNext(out var uid, out var heretic))
        {
            if (heretic.NextPointUpdate <= time)
            {
                heretic.NextPointUpdate = time + heretic.PointCooldown;

                UpdateKnowledge(uid, heretic, 1f);
            }

            if (heretic.AlertTime <= time && heretic.Ascended)
            {
                if (!_transformQuery.TryComp(uid, out var transform))
                    return;

                if (transform.GridUid == null)
                    return;

                var station = _station.GetStationInMap(transform.MapID);

                if (station == null)
                    return;

                _entitySystem.GetEntitySystem<AlertLevelSystem>().SetLevel(station.Value, "delta", true, true, true);
            }
        }
    }

    public void UpdateKnowledge(EntityUid uid, HereticComponent comp, float amount)
    {
        if (TryComp<StoreComponent>(uid, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "KnowledgePoint", amount } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
                objective.Researched += amount;
    }

    private void OnCompInit(Entity<HereticComponent> ent, ref ComponentInit args)
    {
        // add influence layer
        if (TryComp<EyeComponent>(ent, out var eye))
            _eye.SetVisibilityMask(ent, eye.VisibilityMask | EldritchInfluenceComponent.LayerMask);

        foreach (var knowledge in ent.Comp.BaseKnowledge)
            _knowledge.AddKnowledge(ent, ent.Comp, knowledge);

        ent.Comp.NextPointUpdate = _timing.CurTime + ent.Comp.PointCooldown;

        RaiseLocalEvent(ent, new EventHereticRerollTargets());
    }

    #region Internal events (target reroll, ascension, etc.)

    private void OnUpdateTargets(Entity<HereticComponent> ent, ref EventHereticUpdateTargets args)
    {
        ent.Comp.SacrificeTargets = ent.Comp.SacrificeTargets
            .Where(target => TryGetEntity(target, out var tent) && Exists(tent))
            .ToList();
        Dirty<HereticComponent>(ent); // update client
    }

    private void OnRerollTargets(Entity<HereticComponent> ent, ref EventHereticRerollTargets args)
    {
        // welcome to my linq smorgasbord of doom
        // have fun figuring that out

        var targets = _antag.GetAliveConnectedPlayers(_playerMan.Sessions)
            .Where(ics => ics.AttachedEntity.HasValue && HasComp<HumanoidAppearanceComponent>(ics.AttachedEntity));

        var eligibleTargets = new List<EntityUid>();
        foreach (var target in targets)
            eligibleTargets.Add(target.AttachedEntity!.Value); // it can't be null because see .Where(HasValue)

        // no heretics or other baboons
        eligibleTargets = eligibleTargets.Where(t => !HasComp<GhoulComponent>(t) && !HasComp<HereticComponent>(t)).ToList();

        var pickedTargets = new List<EntityUid?>();

        var predicates = new List<Func<EntityUid, bool>>();

        // pick one command staff
        predicates.Add(t => HasComp<CommandStaffComponent>(t));

        // add more predicates here

        foreach (var predicate in predicates)
        {
            var list = eligibleTargets.Where(predicate).ToList();

            if (list.Count == 0)
                continue;

            // pick and take
            var picked = _rand.PickAndTake<EntityUid>(list);
            pickedTargets.Add(picked);
        }

        // add whatever more until satisfied
        for (int i = 0; i <= ent.Comp.MaxTargets - pickedTargets.Count; i++)
            if (eligibleTargets.Count > 0)
                pickedTargets.Add(_rand.PickAndTake<EntityUid>(eligibleTargets));

        // leave only unique entityuids
        pickedTargets = pickedTargets.Distinct().ToList();

        ent.Comp.SacrificeTargets = pickedTargets.ConvertAll(t => GetNetEntity(t)).ToList();
        Dirty<HereticComponent>(ent); // update client
    }

    // notify the crew of how good the person is and play the cool sound :godo:
    private void OnAscension(Entity<HereticComponent> ent, ref EventHereticAscension args)
    {
        ent.Comp.Ascended = true;

        // how???
        if (ent.Comp.CurrentPath == null)
            return;

        var pathLoc = ent.Comp.CurrentPath!.ToLower();
        var ascendSound = new SoundPathSpecifier($"/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/ascend_{pathLoc}.ogg");
        _chat.DispatchGlobalAnnouncement(Loc.GetString($"heretic-ascension-{pathLoc}"), Name(ent), true, ascendSound, Color.Pink);

        ent.Comp.AlertTime = _timing.CurTime + ent.Comp.AlertWaitTime;

        // do other logic, e.g. make heretic immune to whatever
        switch (ent.Comp.CurrentPath!)
        {
            case "Ash":
                RemComp<TemperatureComponent>(ent);
                RemComp<RespiratorComponent>(ent);
                RemComp<BarotraumaComponent>(ent);
                break;

            default:
                break;
        }
    }

    #endregion

    #region External events (damage, etc.)

    private void OnBeforeDamage(Entity<HereticComponent> ent, ref BeforeDamageChangedEvent args)
    {
        // ignore damage from heretic stuff
        if (args.Origin.HasValue && HasComp<HereticBladeComponent>(args.Origin))
            args.Cancelled = true;
    }
    private void OnDamage(Entity<HereticComponent> ent, ref DamageModifyEvent args)
    {
        if (!ent.Comp.Ascended)
            return;

        switch (ent.Comp.CurrentPath)
        {
            case "Ash":
                // nullify heat damage because zased
                args.Damage.DamageDict["Heat"] = 0;
                break;
        }
    }

    #endregion
}
