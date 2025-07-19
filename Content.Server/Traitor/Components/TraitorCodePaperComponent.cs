// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Traitor.Components;

/// <summary>
///     Paper with written traitor codewords on it.
/// </summary>
[RegisterComponent]
public sealed partial class TraitorCodePaperComponent : Component
{
    /// <summary>
    /// The number of codewords that should be generated on this paper.
    /// Will not extend past the max number of available codewords.
    /// </summary>
    [DataField]
    public int CodewordAmount = 1;

    /// <summary>
    /// Whether the codewords should be faked if there is no traitor gamerule set.
    /// </summary>
    [DataField]
    public bool FakeCodewords = true;

    /// <summary>
    /// Whether all codewords added to the round should be used. Overrides CodewordAmount if true.
    /// </summary>
    [DataField]
    public bool CodewordShowAll = false;
}
