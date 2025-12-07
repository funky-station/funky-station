// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     For items that can clean up surgical dirtiness
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryCleansDirtComponent : Component
{
    /// <summary>
    /// How long it takes to wipe prints/blood/etc. off of things using this entity
    /// </summary>
    [DataField]
    public float CleanDelay = 4.0f;

    /// <summary>
    /// How much dirt is removed per clean
    /// </summary>
    [DataField]
    public FixedPoint2 DirtAmount = 5.0;

    /// <summary>
    /// How many DNAs are removed per clean
    /// </summary>
    [DataField]
    public int DnaAmount = 1;
}
