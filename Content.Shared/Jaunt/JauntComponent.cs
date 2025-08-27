// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Jaunt;

/// <summary>
///     Used to control various aspects of a Jaunt.
///     Can be used in place of giving a jaunt-action directly.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class JauntComponent : Component
{
    /// <summary>
    ///     Which Jaunt Action the component should grant.
    /// </summary>
    [DataField]
    public EntProtoId JauntAction = "ActionPolymorphJaunt";

    /// <summary>
    ///     The jaunt action itself.
    /// </summary>
    public EntityUid? Action;

    // TODO: Enter & Exit Times and Whitelist when Actions are reworked and can support it
    // TODO: Cooldown pausing when Actions can support it
}
