// SPDX-FileCopyrightText: 2025 Lyndomen <49795619+Lyndomen@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.Records;

namespace Content.Server._Funkystation.Records;

/// <summary>
/// The component on the station that stores records after the round starts.
/// </summary>
[RegisterComponent]
[Access(typeof(CharacterRecordsSystem))]
public sealed partial class CharacterRecordsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<uint, FullCharacterRecords> Records = new();

    [ViewVariables(VVAccess.ReadOnly)]
    private uint _nextKey = 1;

    /// <summary>
    /// Creates a key has never been used previously
    /// </summary>
    public uint CreateNewKey()
    {
        return _nextKey++;
    }
}

public sealed record CharacterRecordKey
{
    public EntityUid Station { get; init; }
    public uint Index { get; init; }
}
