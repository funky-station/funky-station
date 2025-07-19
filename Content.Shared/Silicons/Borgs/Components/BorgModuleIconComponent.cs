// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

//using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used to override the action icon for cyborg actions.
/// Without this component the no-action state will be used.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BorgModuleIconComponent : Component
{
    /// <summary>
    /// The action icon for this module
    /// </summary>
    [DataField]
    public SpriteSpecifier.Rsi Icon = default!;

}