// SPDX-FileCopyrightText: 2024 HoofedEar <HoofedEar@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.RequiresGrid;

/// <summary>
/// Destroys an entity when they no longer are part of a Grid
/// </summary>
[RegisterComponent]
[Access(typeof(RequiresGridSystem))]
public sealed partial class RequiresGridComponent : Component
{

}
