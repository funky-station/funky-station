// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Ruin offered for the magnet, generated from station maps.
/// </summary>
public record struct RuinOffering : ISalvageMagnetOffering
{
    public RuinMapPrototype RuinMap;
    
    /// <summary>
    /// Generated name for the ruined station
    /// </summary>
    public string StationName;

    public RuinOffering()
    {
        RuinMap = null!;
        StationName = string.Empty;
    }
}

