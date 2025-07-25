// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Database;
using Robust.Shared.Network;

namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player has notes within a certain date range.
/// </summary>
public sealed partial class ConditionNotesDateRange : WhitelistCondition
{
    [DataField]
    public bool IncludeExpired = false;

    [DataField]
    public NoteSeverity MinimumSeverity  = NoteSeverity.Minor;

    /// <summary>
    /// The minimum number of notes required.
    /// </summary>
    [DataField]
    public int MinimumNotes = 1;

    /// <summary>
    /// Range in days to check for notes.
    /// </summary>
    [DataField]
    public int Range = int.MaxValue;

    [DataField]
    public bool IncludeSecret = false;
}
