// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Shared.CartridgeLoader.Cartridges;

/// <summary>
///     Component attached to a piece of paper to indicate that it was printed from NanoTask and can be inserted back into it
/// </summary>
[RegisterComponent]
public sealed partial class NanoTaskPrintedComponent : Component
{
    /// <summary>
    /// The task that this item holds
    /// </summary>
    [DataField]
    public NanoTaskItem? Task;
}
