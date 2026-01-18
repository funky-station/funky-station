// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 KP <13428215+nok-ko@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 LightVillet <dev@null>
// SPDX-FileCopyrightText: 2023 LightVillet <maxim12000@ya.ru>
// SPDX-FileCopyrightText: 2023 Repo <47093363+Titian3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 redfire1331 <Redfire1331@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 0vrseer <iov3rseeri@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Perry Fraser <perryprog@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 PurpleTranStar <tehevilduckiscoming@gmail.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pre-commit-ci[bot] <66853113+pre-commit-ci[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Spillable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Content.Shared._DV.Chemistry.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups; // DeltaV Beergoggles enable safe throw
using Robust.Shared.Physics.Systems; // DeltaV Beergoggles enable safe throw

namespace Content.Server.Fluids.EntitySystems;

public sealed partial class PuddleSystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!; // DeltaV - Beergoggles enable safe throw
    [Dependency] private readonly SafeSolutionThrowerSystem _safesolthrower = default!; // DeltaV - Beergoggles enable safe throw

    protected override void InitializeSpillable()
    {
        base.InitializeSpillable();

        SubscribeLocalEvent<SpillableComponent, LandEvent>(SpillOnLand);
        // Openable handles the event if it's closed
        SubscribeLocalEvent<SpillableComponent, SolutionContainerOverflowEvent>(OnOverflow);
        SubscribeLocalEvent<SpillableComponent, SpillDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<SpillableComponent, AttemptPacifiedThrowEvent>(OnAttemptPacifiedThrow);
    }

    private void OnOverflow(Entity<SpillableComponent> entity, ref SolutionContainerOverflowEvent args)
    {
        if (args.Handled)
            return;

        TrySpillAt(Transform(entity).Coordinates, args.Overflow, out _);
        args.Handled = true;
    }

    private void SpillOnLand(Entity<SpillableComponent> entity, ref LandEvent args)
    {
        if (!_solutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.SolutionName, out var soln, out var solution))
            return;

        if (Openable.IsClosed(entity.Owner))
            return;

        if (!entity.Comp.SpillWhenThrown)
            return;

        if (args.User != null)
        {
            // DeltaV - start of Beergoggles enable safe throw
            if (_safesolthrower.GetSafeThrow(args.User.Value))
            {
                _physics.SetAngularVelocity(entity, 0);
                Transform(entity).LocalRotation = Angle.Zero;
                return;
            }
            // DeltaV - end of Beergoggles enable safe throw
            AdminLogger.Add(LogType.Landed,
                $"{ToPrettyString(entity.Owner):entity} spilled a solution {SharedSolutionContainerSystem.ToPrettyString(solution):solution} on landing");
        }

        var drainedSolution = _solutionContainerSystem.Drain(entity.Owner, soln.Value, solution.Volume);
        TrySplashSpillAt(entity.Owner, Transform(entity).Coordinates, drainedSolution, out _);
    }

    /// <summary>
    /// Prevent Pacified entities from throwing items that can spill liquids.
    /// </summary>
    private void OnAttemptPacifiedThrow(Entity<SpillableComponent> ent, ref AttemptPacifiedThrowEvent args)
    {
        // Don’t care about closed containers.
        if (Openable.IsClosed(ent))
            return;

        // Don’t care about empty containers.
        if (!_solutionContainerSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var solution) || solution.Volume <= 0)
            return;

        // DeltaV - start of Beergoggles enable safe throw
        if (_safesolthrower.GetSafeThrow(args.PlayerUid))
            return;
        // DeltaV - end of Beergoggles enable safe throw
        args.Cancel("pacified-cannot-throw-spill");
    }

    private void OnDoAfter(Entity<SpillableComponent> entity, ref SpillDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        //solution gone by other means before doafter completes
        if (!_solutionContainerSystem.TryGetDrainableSolution(entity.Owner, out var soln, out var solution) || solution.Volume == 0)
            return;

        var puddleSolution = _solutionContainerSystem.SplitSolution(soln.Value, solution.Volume);
        TrySpillAt(Transform(entity).Coordinates, puddleSolution, out _);
        args.Handled = true;
    }
}
