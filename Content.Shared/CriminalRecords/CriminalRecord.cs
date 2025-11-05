// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Эдуард <36124833+Ertanic@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya Mikheev <me@ilyamikcoder.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.Security;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared.CriminalRecords;

/// <summary>
/// Criminal record for a crewmember.
/// Can be viewed and edited in a criminal records console by security.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public sealed partial record CriminalRecord
{
    /// <summary>
    /// Status of the person (None, Wanted, Detained).
    /// </summary>
    [DataField]
    public ProtoId<SecurityStatusPrototype> Status = "SecurityStatusNone";

    /// <summary>
    /// When Status is Wanted, the reason for it.
    /// Should never be set otherwise.
    /// </summary>
    [DataField]
    public string? Reason;

    /// <summary>
    /// The name of the person who changed the status.
    /// </summary>
    [DataField]
    public string? InitiatorName;

    /// <summary>
    /// Criminal history of the person.
    /// This should have charges and time served added after someone is detained.
    /// </summary>
    [DataField]
    public List<CrimeHistory> History = [];
}

/// <summary>
/// A line of criminal activity and the time it was added at.
/// </summary>
[Serializable, NetSerializable]
public record struct CrimeHistory(TimeSpan AddTime, string Crime, string? InitiatorName);
