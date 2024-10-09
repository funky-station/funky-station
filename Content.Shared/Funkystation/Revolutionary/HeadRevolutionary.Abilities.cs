using Content.Shared.Actions;
using Robust.Shared.GameStates;
using static Content.Shared.Revolutionary.HeadRevolutionaryPathComponent;

namespace Content.Shared.Revolutionary;

[RegisterComponent, NetworkedComponent]
public sealed partial class HeadRevolutionaryActionComponent : Component
{
    /// <summary>
    /// Indicates if this actions should be locked by a path. Path defined by name, or
    /// "None" if it is a general ability
    /// </summary>
    [DataField]
    public RevolutionaryPaths RequiresSelectedPath = RevolutionaryPaths.NONE;
}


#region Abilities
public sealed partial class EventHeadRevolutionaryOpenUplink : InstantActionEvent { }
public readonly record struct HeadRevolutionarySelectedVanguardEvent { }
public readonly record struct HeadRevolutionarySelectedWOTPEvent { }
public readonly record struct HeadRevolutionarySelectedWarlordEvent { }

#endregion
