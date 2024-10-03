using Content.Shared.Revolutionary;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Revolutionary;
/// <summary>
/// So why are we doing this in a different component?
/// Well, we wanna be sure that if wizden ever updates this gamemode, we can merge those changes without fear of breaking anything.
///
/// If you don't like it well, no one's asking you to merge this.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HRevComponent : Component
{
    [DataField, AutoNetworkedField]
    public RevolutionaryPaths CurrentPath = RevolutionaryPaths.NONE;

    public enum RevolutionaryPaths
    {
        NONE,
        VANGUARD,
        WOTP,
        WARLORD
    }
}
