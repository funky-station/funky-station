// SPDX-FileCopyrightText: 2026 AftrLite <61218133+AftrLite@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System.Linq;
using Content.Server._DV.CosmicCult.Components;
using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.CosmicCult;

public sealed class MalignRiftSpawnRule : StationEventSystem<MalignRiftSpawnRuleComponent>
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CosmicRiftSystem _malignRift = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;

    protected override void Added(EntityUid uid, MalignRiftSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        AdminLogManager.Add(LogType.EventAnnounced, $"Event added / announced: {ToPrettyString(uid)}");
    }
    protected override void Started(EntityUid uid, MalignRiftSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;

        var grid = StationSystem.GetLargestGrid(stationData);

        if (grid is null)
            return;

        if (_ticker.IsGameRuleActive<CosmicCultRuleComponent>())
        {
            _ticker.EndGameRule(uid); // Cosmic cult's active! Don't actually proceed to the contents of the gamerule!
        }
        else
        {
            var totalCrew = _playerMan.Sessions.Count(session => session.Status == SessionStatus.InGame && HasComp<HumanoidAppearanceComponent>(session.AttachedEntity));
            var sender = Loc.GetString("cosmiccult-announcement-sender");

            _chatSystem.DispatchStationAnnouncement(chosenStation.Value, Loc.GetString("cosmiccult-announce-tier2-progress"), sender, false, null, Color.FromHex("#4cabb3"));
            _chatSystem.DispatchStationAnnouncement(chosenStation.Value, Loc.GetString("cosmiccult-announce-tier2-warning"), null, false, null, Color.FromHex("#cae8e8"));
            _audio.PlayGlobal(comp.Tier2Sound, Filter.Broadcast(), false, AudioParams.Default);

            var lights = EntityQueryEnumerator<PoweredLightComponent>();
            while (lights.MoveNext(out var light, out _))
            {
                if (!_rand.Prob(0.50f))
                    continue;
                _ghost.DoGhostBooEvent(light);
            }

            for (var i = 0; i < Convert.ToInt16(totalCrew / 6); i++) // spawn # malign rifts equal to 16.67% of the playercount
            {
                _malignRift.SpawnRift(grid.Value);
            }
        }
    }
}
