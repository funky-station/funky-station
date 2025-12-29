// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 sBasalto <109002990+sBasalto@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vyacheslav Titov <rincew1nd@ya.ru>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 ArchPigeon <bookmaster3@gmail.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Wrexbe (Josh) <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JoulesBerg <104539820+JoulesBerg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 ferynn <117872973+ferynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ferynn <witchy.girl.me@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Server.Zombies;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Zombies;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Globalization;

namespace Content.Server.GameTicking.Rules;

public sealed class ZombieRuleSystem : GameRuleSystem<ZombieRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly ZombieTumorOrganSystem _zombieTumor = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InitialInfectedRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<ZombieRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<IncurableZombieComponent, ZombifySelfActionEvent>(OnZombifySelf);
        SubscribeLocalEvent<ZombieRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    private void OnGetBriefing(Entity<InitialInfectedRoleComponent> role, ref GetBriefingEvent args)
    {
        if (!_roles.MindHasRole<ZombieRoleComponent>(args.Mind.Owner))
            args.Append(Loc.GetString("zombie-patientzero-role-greeting"));
    }

    private void OnGetBriefing(Entity<ZombieRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("zombie-infection-greeting"));
    }

    protected override void AppendRoundEndText(EntityUid uid,
        ZombieRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        // This is just the general condition thing used for determining the win/lose text
        var fraction = GetInfectedFraction(true, true);

        if (fraction <= 0)
            args.AddLine(Loc.GetString("zombie-round-end-amount-none"));
        else if (fraction <= 0.25)
            args.AddLine(Loc.GetString("zombie-round-end-amount-low"));
        else if (fraction <= 0.5)
            args.AddLine(Loc.GetString("zombie-round-end-amount-medium", ("percent", Math.Round((fraction * 100), 2).ToString(CultureInfo.InvariantCulture))));
        else if (fraction < 1)
            args.AddLine(Loc.GetString("zombie-round-end-amount-high", ("percent", Math.Round((fraction * 100), 2).ToString(CultureInfo.InvariantCulture))));
        else
            args.AddLine(Loc.GetString("zombie-round-end-amount-all"));

        var antags = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("zombie-round-end-initial-count", ("initialCount", antags.Count)));
        foreach (var (_, data, entName) in antags)
        {
            args.AddLine(Loc.GetString("zombie-round-end-user-was-initial",
                ("name", entName),
                ("username", data.UserName)));
        }

        var healthy = GetHealthyHumans();
        // Gets a bunch of the living players and displays them if they're under a threshold.
        // InitialInfected is used for the threshold because it scales with the player count well.
        if (healthy.Count <= 0 || healthy.Count > 2 * antags.Count)
            return;
        args.AddLine("");
        args.AddLine(Loc.GetString("zombie-round-end-survivor-count", ("count", healthy.Count)));
        foreach (var survivor in healthy)
        {
            var meta = MetaData(survivor);
            var username = string.Empty;
            if (_mindSystem.TryGetMind(survivor, out _, out var mind) &&
                _player.TryGetSessionById(mind.UserId, out var session))
            {
                username = session.Name;
            }

            args.AddLine(Loc.GetString("zombie-round-end-user-was-survivor",
                ("name", meta.EntityName),
                ("username", username)));
        }
        args.AddLine("");
    }

    /// <summary>
    ///     The big kahoona function for checking if the round is gonna end
    /// </summary>
    private void CheckRoundEnd(ZombieRuleComponent zombieRuleComponent)
    {
        var healthy = GetHealthyHumans();
        if (healthy.Count == 1) // Only one human left. spooky
            _popup.PopupEntity(Loc.GetString("zombie-alone"), healthy[0], healthy[0]);

        if (GetInfectedFraction(false) > zombieRuleComponent.ZombieShuttleCallPercentage && !_roundEnd.IsRoundEndRequested())
        {
            foreach (var station in _station.GetStations())
            {
                _chat.DispatchStationAnnouncement(station, Loc.GetString("zombie-shuttle-call"), colorOverride: Color.Crimson);
            }
            _roundEnd.RequestRoundEnd(null, false);
        }

        // we include dead for this count because we don't want to end the round
        // when everyone gets on the shuttle.
        if (GetInfectedFraction() >= 1) // Oops, all zombies
            _roundEnd.EndRound();

        // Check if 80% of connected players are zombified, dead, or ghosts
        if (CheckZombieVictoryPercentage() && !_roundEnd.IsRoundEndRequested())
            _roundEnd.EndRound();
    }

    protected override void Started(EntityUid uid, ZombieRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.NextRoundEndCheck = _timing.CurTime + component.EndCheckDelay;
    }

    protected override void ActiveTick(EntityUid uid, ZombieRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);
        if (!component.NextRoundEndCheck.HasValue || component.NextRoundEndCheck > _timing.CurTime)
            return;
        CheckRoundEnd(component);
        component.NextRoundEndCheck = _timing.CurTime + component.EndCheckDelay;
    }

    private void OnZombifySelf(EntityUid uid, IncurableZombieComponent component, ZombifySelfActionEvent args)
    {
        _zombie.ZombifyEntity(uid);
        if (component.Action != null)
            Del(component.Action.Value);
    }

    private void OnAntagSelected(Entity<ZombieRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        // Apply zombie tumor infection to selected players instead of InitialInfected component
        // Start at Incubation stage so infection doesn't spread immediately
        _zombieTumor.InfectEntity(args.EntityUid, ZombieTumorInfectionStage.Incubation);
    }

    /// <summary>
    /// Get the fraction of players that are infected, between 0 and 1
    /// </summary>
    /// <param name="includeOffStation">Include healthy players that are not on the station grid</param>
    /// <param name="includeDead">Should dead zombies be included in the count</param>
    /// <returns></returns>
    private float GetInfectedFraction(bool includeOffStation = true, bool includeDead = false)
    {
        var players = GetHealthyHumans(includeOffStation);
        var zombieCount = 0;
        var query = EntityQueryEnumerator<HumanoidAppearanceComponent, ZombieComponent, MobStateComponent>();
        while (query.MoveNext(out _, out _, out _, out var mob))
        {
            if (!includeDead && mob.CurrentState == MobState.Dead)
                continue;
            zombieCount++;
        }

        return zombieCount / (float) (players.Count + zombieCount);
    }

    /// <summary>
    /// Gets the list of humans who are alive, not zombies, and are on a station.
    /// Flying off via a shuttle disqualifies you.
    /// </summary>
    /// <returns></returns>
    private List<EntityUid> GetHealthyHumans(bool includeOffStation = true)
    {
        var healthy = new List<EntityUid>();

        var stationGrids = new HashSet<EntityUid>();
        if (!includeOffStation)
        {
            foreach (var station in _station.GetStationsSet())
            {
                if (_station.GetLargestGrid(station) is { } grid)
                    stationGrids.Add(grid);
            }
        }

        var players = AllEntityQuery<HumanoidAppearanceComponent, ActorComponent, MobStateComponent, TransformComponent>();
        var zombers = GetEntityQuery<ZombieComponent>();
        while (players.MoveNext(out var uid, out _, out _, out var mob, out var xform))
        {
            if (!_mobState.IsAlive(uid, mob))
                continue;

            if (zombers.HasComponent(uid))
                continue;

            if (!includeOffStation && !stationGrids.Contains(xform.GridUid ?? EntityUid.Invalid))
                continue;

            healthy.Add(uid);
        }
        return healthy;
    }

    /// <summary>
    /// Checks if 80% of all currently connected players are zombified, dead, or ghosts.
    /// Returns true if zombie victory condition is met (only 20% or less are alive living humanoids).
    /// </summary>
    private bool CheckZombieVictoryPercentage()
    {
        var totalConnectedPlayers = 0;
        var zombifiedDeadOrGhostCount = 0;

        // Count all connected players
        foreach (var session in _playerManager.Sessions)
        {
            // Skip if player has no attached entity
            if (session.AttachedEntity == null)
                continue;

            var entity = session.AttachedEntity.Value;
            totalConnectedPlayers++;

            // Check if player is a zombie
            if (HasComp<ZombieComponent>(entity))
            {
                zombifiedDeadOrGhostCount++;
                continue;
            }

            // Check if player is dead
            if (TryComp<MobStateComponent>(entity, out var mobState) && _mobState.IsDead(entity, mobState))
            {
                zombifiedDeadOrGhostCount++;
                continue;
            }

            // Check if player is a ghost
            if (HasComp<GhostComponent>(entity))
            {
                zombifiedDeadOrGhostCount++;
                continue;
            }
        }

        // Need at least 1 connected player to check
        if (totalConnectedPlayers == 0)
            return false;

        // Check if 80% or more are zombified/dead/ghosts (20% or less are alive living humanoids)
        var percentage = (float)zombifiedDeadOrGhostCount / totalConnectedPlayers;
        return percentage >= 0.8f;
    }
}
