// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;

namespace Content.Server.Explosion.Components;

/// <summary>
/// Checks if the user of a Trigger satisfies a whitelist and blacklist condition.
/// Cancels the trigger otherwise.
/// </summary>
[RegisterComponent]
public sealed partial class TriggerWhitelistComponent : Component
{
    /// <summary>
    /// Whitelist for what entites can cause this trigger.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Blacklist for what entites can cause this trigger.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
