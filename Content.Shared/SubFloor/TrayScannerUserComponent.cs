// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.SubFloor;

// Don't need to network
/// <summary>
/// Added to anyone using <see cref="TrayScannerComponent"/> to handle the vismask changes.
/// </summary>
[RegisterComponent]
public sealed partial class TrayScannerUserComponent : Component
{
    /// <summary>
    /// How many t-rays the user is currently using.
    /// </summary>
    [DataField]
    public int Count;
}
