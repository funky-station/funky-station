// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.BanList;

[Serializable, NetSerializable]
public sealed record SharedServerRoleBan(
    int? Id,
    NetUserId? UserId,
    (string address, int cidrMask)? Address,
    string? HWId,
    DateTime BanTime,
    DateTime? ExpirationTime,
    string Reason,
    string? BanningAdminName,
    SharedServerUnban? Unban,
    string Role
) : SharedServerBan(Id, UserId, Address, HWId, BanTime, ExpirationTime, Reason, BanningAdminName, Unban);
