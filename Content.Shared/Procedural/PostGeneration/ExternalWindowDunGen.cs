// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// If external areas are found will try to generate windows.
/// </summary>
/// <remarks>
/// Dungeon data keys are:
/// - EntranceFlank
/// - FallbackTile
/// </remarks>
public sealed partial class ExternalWindowDunGen : IDunGenLayer;
