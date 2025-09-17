using Content.Shared.MalfAI;  // MalfAiControlledComponent
using Content.Shared.Actions.Events; // Correct namespace for MalfAiRoboticsFactoryActionEvent
using Content.Server._Funkystation.Factory.Systems;
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

    private static readonly EntProtoId RoboticsFactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiRoboticsFactoryActionEvent>(OnRoboticsFactory);
    }

    private void OnRoboticsFactory(EntityUid uid, MalfAiMarkerComponent comp, ref MalfAiRoboticsFactoryActionEvent args)
    {

        if (args.Handled)
        {
            return;
        }

        // Check if target coordinates are valid
        if (!args.Target.IsValid(EntityManager))
        {
            return;
        }

        // Server determines the prototype - client cannot specify it for security
        var buildRequest = new AIBuildRequestEvent(uid, args.Target, RoboticsFactoryPrototype.Id);

        // Send the build request to the AIBuild system
        RaiseLocalEvent(buildRequest);

        args.Handled = true; // consume the action
    }

}
