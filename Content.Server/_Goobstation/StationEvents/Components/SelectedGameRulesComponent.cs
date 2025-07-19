// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Server._Goobstation.StationEvents.Components;


/// <summary>
///   All the events that are allowed to run in the current round. If this is not assigned to the game rule it will select from all of them :fire:
/// </summary>
[RegisterComponent]
public sealed partial class SelectedGameRulesComponent : Component
{
    /// <summary>
    ///   All the events that are allowed to run in the current round.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
