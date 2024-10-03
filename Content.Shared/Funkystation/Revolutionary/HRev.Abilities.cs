using Content.Shared.Actions;
using Robust.Shared.GameStates;
using static Content.Shared.Revolutionary.HRevComponent;

namespace Content.Shared.Revolutionary;

[RegisterComponent, NetworkedComponent]
public sealed partial class HRevActionComponent : Component
{
    /// <summary>
    /// Indicates if this actions should be locked by a path. Path defined by name, or
    /// "None" if it is a general ability
    /// </summary>
    [DataField]
    public RevolutionaryPaths RequiresSelectedPath = RevolutionaryPaths.NONE;
}


#region Abilities
public sealed partial class EventHRevOpenStore : InstantActionEvent { }
public readonly record struct HRevSelectedVanguardEvent { }
public readonly record struct HRevSelectedWOTPEvent { }
public readonly record struct HRevSelectedWarlordEvent { }

#endregion
