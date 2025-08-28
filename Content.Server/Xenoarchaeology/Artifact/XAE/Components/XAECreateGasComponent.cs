// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos;

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// XenoArtifact effect that creates gas in atmosphere.
/// </summary>
[RegisterComponent, Access(typeof(XAECreateGasSystem))]
public sealed partial class XAECreateGasComponent : Component
{
    /// <summary>
    /// The gases and how many moles will be created of each.
    /// </summary>
    [DataField]
    public Dictionary<Gas, float> Gases = new();
}
