// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DVDPlayerOfDiscordFame <drew.c.bergen@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.FixedPoint;
using Content.Shared.Whitelist; // imp
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReagentTankComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 TransferAmount { get; set; } = FixedPoint2.New(10);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ReagentTankType TankType { get; set; } = ReagentTankType.Unspecified;

    // imp start
    // white list and black list function on the tanks lets them ensure that only the intended tools/equipment can refuel at them.
    [DataField]
    public EntityWhitelist? FuelWhitelist;

    [DataField]
    public EntityWhitelist? FuelBlacklist;
    // imp end
}

[Serializable, NetSerializable]
public enum ReagentTankType : byte
{
    Unspecified,
    Fuel
}
