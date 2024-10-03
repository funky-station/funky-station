using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Revolutionary;

public enum RevolutionaryPaths
{
    NONE,
    VANGUARD,
    WOTP,
    WARLORD
}

[RegisterComponent, NetworkedComponent]
public sealed partial class HRevActionComponent : Component
{
    /// <summary>
    /// Indicates if this actions should be locked by a path. Path defined by name, or
    /// "None" if it is a general ability
    /// </summary>
    [DataField] public RevolutionaryPaths RequiresSelectedPath = RevolutionaryPaths.NONE;
}


#region Abilities
public sealed partial class EventHrevOpenStore : InstantActionEvent { }

#endregion
