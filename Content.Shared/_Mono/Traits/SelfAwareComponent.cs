using Robust.Shared.GameStates;

namespace Content.Shared.Traits;

/// <summary>
/// This component allows the entity to examine their own damage like a health analyzer
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SelfAwareComponent : Component
{
} 
