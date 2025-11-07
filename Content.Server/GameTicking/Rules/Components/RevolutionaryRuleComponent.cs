// SPDX-FileCopyrightText: 2023 JoeHammad1844 <130668733+JoeHammad1844@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

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
    public int StartingBalance = 55;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId UplinkStoreId = "StorePresetRevolutionaryUplink";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId UplinkCurrencyId = "RevCoin";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool OpenRevoltDeclared = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool OpenRevoltAnnouncementPending = false;
}
