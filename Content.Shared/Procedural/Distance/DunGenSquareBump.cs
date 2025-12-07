// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.Distance;

/// <summary>
/// Produces a squarish-shape that's better for filling in most of the area.
/// </summary>
public sealed partial class DunGenSquareBump : IDunGenDistance
{
    [DataField]
    public float BlendWeight { get; set; } = 0.50f;
}
