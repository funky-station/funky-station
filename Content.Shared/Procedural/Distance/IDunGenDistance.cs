// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.Distance;

/// <summary>
/// Used if you want to limit the distance noise is generated by some arbitrary config
/// </summary>
[ImplicitDataDefinitionForInheritors]
public partial interface IDunGenDistance
{
    /// <summary>
    /// How much to blend between the original noise value and the adjusted one.
    /// </summary>
    float BlendWeight { get; }
}

