// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Mind;

/// <summary>
///     The core properties of Role Types
/// </summary>
[Prototype, Serializable]
public sealed partial class RoleTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The role's name as displayed on the UI.
    /// </summary>
    [DataField]
    public LocId Name = "role-type-crew-aligned-name";

    /// <summary>
    ///     The role's displayed color.
    /// </summary>
    [DataField]
    public Color Color { get; private set; } = Color.FromHex("#eeeeee");

    /// <summary>
    ///     A symbol used to represent the role type.
    /// </summary>
    [DataField]
    public string Symbol = string.Empty;
}
