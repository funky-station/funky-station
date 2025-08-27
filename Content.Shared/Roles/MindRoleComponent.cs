// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mind;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

/// <summary>
/// This holds data for, and indicates, a Mind Role entity
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MindRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    ///     Marks this Mind Role as Antagonist
    ///     A single antag Mind Role is enough to make the owner mind count as Antagonist.
    /// </summary>
    [DataField]
    public bool Antag { get; set; } = false;

    /// <summary>
    ///     The mind's current antagonist/special role, or lack thereof;
    /// </summary>
    [DataField]
    public ProtoId<RoleTypePrototype>? RoleType;

    /// <summary>
    ///     True if this mindrole is an exclusive antagonist. Antag setting is not checked if this is True.
    /// </summary>
    [DataField]
    public bool ExclusiveAntag { get; set; } = false;

    /// <summary>
    ///     The Mind that this role belongs to
    /// </summary>
    public Entity<MindComponent> Mind { get; set; }

    /// <summary>
    ///     The Antagonist prototype of this role
    /// </summary>
    [DataField]
    public ProtoId<AntagPrototype>? AntagPrototype { get; set; }

    /// <summary>
    ///     The Job prototype of this role
    /// </summary>
    [DataField]
    public ProtoId<JobPrototype>? JobPrototype { get; set; }

    /// <summary>
    ///     Used to order the characters on by role/antag status. Highest numbers are shown first.
    /// </summary>
    [DataField]
    public int SortWeight;
}

// Why does this base component actually exist? It does make auto-categorization easy, but before that it was useless?
// I used it for easy organisation/bookkeeping of what components are for mindroles
[EntityCategory("Roles")]
public abstract partial class BaseMindRoleComponent : Component
{

}
