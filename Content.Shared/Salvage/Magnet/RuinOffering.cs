// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Ruin offered for the magnet, generated from station maps.
/// </summary>
public record struct RuinOffering : ISalvageMagnetOffering
{
    public RuinMapPrototype RuinMap;
}

