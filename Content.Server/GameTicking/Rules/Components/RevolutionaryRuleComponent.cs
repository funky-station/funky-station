using Content.Server.RoundEnd;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the RevolutionaryRuleSystem that stores info about winning/losing, player counts required for starting, as well as prototypes for Revolutionaries and their gear.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem))]
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
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(3);

    // goob edit start
    [DataField] public bool HasAnnouncementPlayed = false;
    [DataField] public bool HasRevAnnouncementPlayed = false;
    // gobo edit end

    // funky station
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? RevVictoryEndTime;

    // funky station
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan RevVictoryEndDelay = TimeSpan.FromMinutes(2);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? RevLoseTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan OffStationTimer = TimeSpan.FromMinutes(1);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RevLossTimerActive = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RevForceLose = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int StartingBalance = 40;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId UplinkStoreId = "StorePresetRevolutionaryUplink";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId UplinkCurrencyId = "RevCoin";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool OpenRevoltDeclared = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool OpenRevoltAnnouncementPending = false;
}
