using Content.Shared.MalfAI;  // MalfAiControlledComponent
using Content.Shared.Actions;  // Action events are defined here
using Content.Shared.Actions.Events; // Correct namespace for MalfAiRoboticsFactoryActionEvent
using Content.Server._Funkystation.Factory.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Robotics Factory action by requesting the AIBuild system to spawn a RoboticsFactoryGrid.
/// The server knows exactly what prototype to spawn (RoboticsFactoryGrid) for security.
/// </summary>
public sealed partial class MalfAiRoboticsFactorySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("malf.ai.factory");

    private const string RoboticsFactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiRoboticsFactoryActionEvent>(OnRoboticsFactory);

        var actionPrototypeExists = _prototypes.HasIndex<EntityPrototype>("ActionMalfAiRoboticsFactory");
        var factoryPrototypeExists = _prototypes.HasIndex<EntityPrototype>(RoboticsFactoryPrototype);

        Sawmill.Info($"[DEBUG_LOG] MalfAiRoboticsFactorySystem initialized");
        Sawmill.Info($"[DEBUG_LOG] Action prototype 'ActionMalfAiRoboticsFactory' exists: {actionPrototypeExists}");
        Sawmill.Info($"[DEBUG_LOG] Factory prototype '{RoboticsFactoryPrototype}' exists: {factoryPrototypeExists}");
        Sawmill.Info($"[DEBUG_LOG] Event subscription registered for MalfAiRoboticsFactoryActionEvent");
    }

    private void OnRoboticsFactory(EntityUid uid, MalfAiMarkerComponent comp, ref MalfAiRoboticsFactoryActionEvent args)
    {
        Sawmill.Info($"[DEBUG_LOG] OnRoboticsFactory called - Entity: {ToPrettyString(uid)}, Target: {args.Target}");
        Sawmill.Info($"[DEBUG_LOG] Action event details - Handled: {args.Handled}, HasComp: {HasComp<MalfAiMarkerComponent>(uid)}");

        if (args.Handled)
        {
            Sawmill.Warning($"[DEBUG_LOG] Action already handled, skipping processing");
            return;
        }

        // Check if target coordinates are valid
        if (!args.Target.IsValid(EntityManager))
        {
            Sawmill.Error($"[DEBUG_LOG] Invalid target coordinates: {args.Target}");
            return;
        }

        // Server determines the prototype - client cannot specify it for security
        var buildRequest = new AIBuildRequestEvent(uid, args.Target, RoboticsFactoryPrototype);
        Sawmill.Info($"[DEBUG_LOG] Created AIBuildRequestEvent - Requester: {ToPrettyString(uid)}, Prototype: {RoboticsFactoryPrototype}");

        // Send the build request to the AIBuild system
        RaiseLocalEvent(buildRequest);
        Sawmill.Info($"[DEBUG_LOG] AIBuildRequestEvent raised successfully");

        Sawmill.Info($"[DEBUG_LOG] RoboticsFactory: Build request sent by {ToPrettyString(uid)} for '{RoboticsFactoryPrototype}' at {args.Target}.");
        args.Handled = true; // consume the action
        Sawmill.Info($"[DEBUG_LOG] Action marked as handled");
    }

}
