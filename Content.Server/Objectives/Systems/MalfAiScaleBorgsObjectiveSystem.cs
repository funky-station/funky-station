// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Components;
using Content.Shared.MalfAI;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Localization;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles scaling the required number of borgs based on player count for Malf AI objectives.
/// </summary>
public sealed class MalfAiScaleBorgsObjectiveSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("malfaiobj");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiScaleBorgsObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<MalfAiScaleBorgsObjectiveComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MalfAiScaleBorgsObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, ref ObjectiveAfterAssignEvent args)
    {
        // Calculate target based on current player count
        var playerCount = _playerManager.PlayerCount;
        var targetBorgs = Math.Max(component.MinBorgs, Math.Min(component.MaxBorgs,
            (int)Math.Ceiling((float)playerCount / component.PlayersPerBorg)));

        component.Target = targetBorgs;

        Sawmill.Info($"[MalfAiScaleBorgsObjectiveSystem] OnAfterAssign: Set Target to {component.Target} for entity {uid} (playerCount={playerCount})");

        // Update the objective name to include the number of borgs
        var name = Loc.GetString("malfai-objective-control-borgs", ("count", targetBorgs));
        _metaData.SetEntityName(uid, name);
        // Initialize target/name right when the objective is assigned so the UI shows the correct target immediately.
    }

    private void OnStartup(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, ComponentStartup args)
    {
        CalculateAndSetTarget(uid, component, "OnStartup");
    }

    private void CalculateAndSetTarget(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, string eventName)
    {
        // Calculate target based on current player count
        var playerCount = _playerManager.PlayerCount;
        var targetBorgs = Math.Max(component.MinBorgs, Math.Min(component.MaxBorgs,
            (int)Math.Ceiling((float)playerCount / component.PlayersPerBorg)));

        component.Target = targetBorgs;

        // Debug log to confirm Target is set
        Sawmill.Info($"[MalfAiScaleBorgsObjectiveSystem] {eventName}: Set Target to {component.Target} for entity {uid} (playerCount={playerCount})");

        // Update the objective name to include the number of borgs
        var name = Loc.GetString("malfai-objective-control-borgs", ("count", targetBorgs));
        _metaData.SetEntityName(uid, name);
    }

    private void OnGetProgress(EntityUid uid, MalfAiScaleBorgsObjectiveComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (args.MindId == null)
        {
            args.Progress = 0.0f;
            return;
        }

        // Fallback: If Target is not set, set it to MinBorgs
        if (component.Target <= 0)
        {
            Sawmill.Warning($"[MalfAiScaleBorgsObjectiveSystem] OnGetProgress: Target was {component.Target}, resetting to MinBorgs {component.MinBorgs} for entity {uid}");
            component.Target = component.MinBorgs;
        }

        // Count cyborgs controlled by this AI mind
        var controlledCount = 0;
        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out var borgUid, out var controlled))
        {
            if (controlled.Controller == null)
                continue;

            // Check if the controller matches the objective's mind directly
            if (TryComp<MindContainerComponent>(controlled.Controller.Value, out var controllerMind) &&
                controllerMind.Mind == args.MindId)
            {
                controlledCount++;
            }
        }

        // Calculate progress as controlled borgs / target borgs
        args.Progress = component.Target > 0 ? Math.Min(1.0f, (float)controlledCount / component.Target) : 1.0f;

        // Debug log to confirm progress calculation
        Sawmill.Info($"[MalfAiScaleBorgsObjectiveSystem] OnGetProgress: controlledCount={controlledCount}, Target={component.Target}, Progress={args.Progress} for entity {uid}");
    }
}
