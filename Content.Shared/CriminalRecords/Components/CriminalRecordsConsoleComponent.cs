// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya Mikheev <me@ilyamikcoder.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CriminalRecords.Systems;
using Content.Shared.CriminalRecords.Components;
using Content.Shared.CriminalRecords;
using Content.Shared.Radio;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Security;

namespace Content.Shared.CriminalRecords.Components;

/// <summary>
/// A component for Criminal Record Console storing an active station record key and a currently applied filter
/// </summary>
[RegisterComponent]
[Access(typeof(SharedCriminalRecordsConsoleSystem))]
public sealed partial class CriminalRecordsConsoleComponent : Component
{
    /// <summary>
    /// Currently active station record key.
    /// There is no station parameter as the console uses the current station.
    /// </summary>
    /// <remarks>
    /// TODO: in the future this should be clientside instead of something players can fight over.
    /// Client selects a record and tells the server the key it wants records for.
    /// Server then sends a state with just the records, not the listing or filter, and the client updates just that.
    /// I don't know if it's possible to have multiple bui states right now.
    /// </remarks>
    [DataField]
    public uint? ActiveKey;

    /// <summary>
    /// Currently applied filter.
    /// </summary>
    [DataField]
    public StationRecordsFilter? Filter;

    /// <summary>
    /// Current selected security status for the filter by criminal status dropdown.
    /// </summary>
    [DataField]
    public SecurityStatusPrototype? FilterStatus;

    /// <summary>
    /// Channel to send messages to when someone's status gets changed.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> SecurityChannel = "Security";

    /// <summary>
    /// Max length of arrest and crime history strings.
    /// </summary>
    [DataField]
    public uint MaxStringLength = 256;
}
