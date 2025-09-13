using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Component for handling protect target selection, prioritizing traitors and traitor targets.
/// </summary>
[RegisterComponent, Access(typeof(MalfAiPickProtectTargetSystem))]
public sealed partial class MalfAiPickProtectTargetComponent : Component
{
}
