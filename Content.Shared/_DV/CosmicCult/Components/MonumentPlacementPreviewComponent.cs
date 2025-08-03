// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// a marker component used as an extra flag for an event to toggle the monument preview.
/// could probably have a better name but idrk.
/// </summary>
[RegisterComponent]
public sealed partial class MonumentPlacementPreviewComponent : Component
{
    /// <summary>
    /// the tier of the monument that the overlay added by the event with this comp should render
    /// </summary>
    [DataField]
    public int Tier = 1;
}
