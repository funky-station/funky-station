// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Destructible.Thresholds;

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// Artifact that ignites surrounding entities when triggered.
/// </summary>
[RegisterComponent, Access(typeof(XAEIgniteSystem))]
public sealed partial class XAEIgniteComponent : Component
{
    /// <summary>
    /// Range, inside which all entities going be set on fire.
    /// </summary>
    [DataField]
    public float Range = 2f;

    /// <summary>
    /// Amount of fire stacks to apply
    /// </summary>
    [DataField]
    public MinMax FireStack = new(2, 5);
}
