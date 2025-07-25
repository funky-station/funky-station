// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Spawns entities inside corners.
/// </summary>
/// <remarks>
/// Dungeon data keys are:
/// - CornerClutter
/// </remarks>
public sealed partial class CornerClutterDunGen : IDunGenLayer
{
    [DataField]
    public float Chance = 0.50f;
}
