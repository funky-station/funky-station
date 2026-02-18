// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 AftrLite <61218133+AftrLite@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Antag;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicChantrySystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    /// <summary>
    /// Mind role to add to colossi.
    /// </summary>
    public static readonly EntProtoId MindRole = "MindRoleCosmicColossus";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicChantryComponent, ComponentInit>(OnChantryStarted);
        SubscribeLocalEvent<CosmicChantryComponent, ComponentShutdown>(OnChantryDestroyed);

        SubscribeLocalEvent<CosmicChantryComponent, CosmicChantryDoAfter>(OnDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var chantryQuery = EntityQueryEnumerator<CosmicChantryComponent>();
        while (chantryQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.SpawnTimer && !comp.Spawned)
            {
                _appearance.SetData(uid, ChantryVisuals.Status, ChantryStatus.On);
                _popup.PopupCoordinates(Loc.GetString("cosmiccult-chantry-powerup"), Transform(uid).Coordinates, PopupType.LargeCaution);
                comp.Spawned = true;

                var doAfterArgs = new DoAfterArgs(EntityManager, uid, comp.EventTime, new CosmicChantryDoAfter(), uid, comp.InternalVictim)
                {
                    NeedHand = false,
                    BreakOnWeightlessMove = false,
                    BreakOnMove = false,
                    BreakOnHandChange = false,
                    BreakOnDropItem = false,
                    BreakOnDamage = false,
                    RequireCanInteract = false,
                };
                _doAfter.TryStartDoAfter(doAfterArgs);
            }
        }
    }

    private void OnDoAfter(Entity<CosmicChantryComponent> ent, ref CosmicChantryDoAfter args)
    {
        if (!_mind.TryGetMind(ent.Comp.InternalVictim, out var mindEnt, out var mind))
            return;
        mind.PreventGhosting = false;
        var tgtpos = Transform(ent).Coordinates;
        var colossus = Spawn(ent.Comp.Colossus, tgtpos);
        _mind.TransferTo(mindEnt, colossus);
        _mind.TryAddObjective(mindEnt, mind, "CosmicFinalityObjective");
        _role.MindAddRole(mindEnt, MindRole, mind, true);
        _antag.SendBriefing(colossus, Loc.GetString("cosmiccult-silicon-colossus-briefing"), Color.FromHex("#4cabb3"), null);
        Spawn(ent.Comp.SpawnVFX, tgtpos);
        QueueDel(ent.Comp.InternalVictim);
        QueueDel(ent);
    }

    private void OnChantryStarted(Entity<CosmicChantryComponent> ent, ref ComponentInit args)
    {
        var indicatedLocation = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((ent, Transform(ent))));
        var comp = ent.Comp;

        comp.SpawnTimer = _timing.CurTime + comp.SpawningTime;
        comp.CountdownTimer = _timing.CurTime + comp.EventTime;

        _sound.PlayGlobalOnStation(ent, _audio.ResolveSound(comp.ChantryAlarm));
        _chatSystem.DispatchStationAnnouncement(ent,
        Loc.GetString("cosmiccult-chantry-location", ("location", indicatedLocation)),
        null, false, null,
        Color.FromHex("#cae8e8"));

        if (_mind.TryGetMind(comp.InternalVictim, out _, out var mind))
            mind.PreventGhosting = true;
    }

    private void OnChantryDestroyed(Entity<CosmicChantryComponent> ent, ref ComponentShutdown args)
    {
        var comp = ent.Comp;
        if (!_mind.TryGetMind(comp.InternalVictim, out var mindId, out var mind))
            return;
        if (TerminatingOrDeleted(comp.VictimBody))
        {
            var tgtpos = Transform(comp.InternalVictim).Coordinates;
            var fallbackEnt = Spawn(comp.FallbackBrain, tgtpos);
            Spawn(comp.FallbackVFX, tgtpos);
            mind.PreventGhosting = false;
            _mind.TransferTo(mindId, fallbackEnt);
            QueueDel(comp.InternalVictim);
        }
        else
        {
            mind.PreventGhosting = false;
            _mind.TransferTo(mindId, comp.VictimBody);
            QueueDel(comp.InternalVictim);
        }
    }
}
