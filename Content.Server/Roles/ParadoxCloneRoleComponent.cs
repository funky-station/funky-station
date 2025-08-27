// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are a paradox clone.
/// </summary>
[RegisterComponent]
public sealed partial class ParadoxCloneRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    ///     Name modifer applied to the player when they turn into a ghost.
    ///     Needed to be able to keep the original and the clone apart in dead chat.
    /// </summary>
    [DataField]
    public LocId? NameModifier = "paradox-clone-ghost-name-modifier";
}
