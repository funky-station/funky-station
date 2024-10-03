using Content.Server.RoundEnd;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Store;
using Content.Shared.Revolutionary;
using Content.Server.Revolutionary;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the RevolutionaryRuleSystem that stores info about winning/losing, player counts required for starting, as well as prototypes for Revolutionaries and their gear.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem), typeof(HRevSystem))]
public sealed partial class RevolutionaryRuleComponent : Component
{
    /// <summary>
    /// When the round will if all the command are dead (Incase they are in space)
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CommandCheck;

    /// <summary>
    /// The amount of time between each check for command check.
    /// </summary>
    [DataField]
    public TimeSpan TimerWait = TimeSpan.FromSeconds(20);

    /// <summary>
    /// The time it takes after the last head is killed for the shuttle to arrive.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(5);

    // goob edit start
    [DataField] public bool HasAnnouncementPlayed = false;
    [DataField] public bool HasRevAnnouncementPlayed = false;
    // gobo edit end

    // funkystation edit
    [ViewVariables(VVAccess.ReadWrite)]
    public readonly Dictionary<RevolutionaryPaths, ProtoId<StoreCategoryPrototype>> RevCoinStore = new()
    {
        {
            RevolutionaryPaths.NONE,
            "RevStoreGeneral"
        },
        {
            RevolutionaryPaths.VANGUARD,
            "RevStoreVanguard"
        },
        {
            RevolutionaryPaths.WARLORD,
            "RevStoreWarlord"
        },
        {
            RevolutionaryPaths.WOTP,
            "RevStoreWotp"
        },
    };
}
