// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CriminalRecords.Systems;
using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.CriminalRecords.Components;

/// <summary>
/// Lets the user hack a criminal records console, once.
/// Everyone is set to wanted with a randomly picked reason.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCriminalRecordsHackerSystem))]
public sealed partial class CriminalRecordsHackerComponent : Component
{
    /// <summary>
    /// How long the doafter is for hacking it.
    /// </summary>
    public TimeSpan Delay = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Dataset of random reasons to use.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> Reasons = "CriminalRecordsWantedReasonPlaceholders";

    /// <summary>
    /// Announcement made after the console is hacked.
    /// </summary>
    [DataField]
    public LocId Announcement = "ninja-criminal-records-hack-announcement";
}
