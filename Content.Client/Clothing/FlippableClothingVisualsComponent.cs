// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Clothing;

/// <summary>
/// Communicates folded layers data (currently only Scale to handle flipping)
/// to the wearer clothing sprite layer
/// </summary>
[RegisterComponent]
[Access(typeof(FlippableClothingVisualizerSystem))]
public sealed partial class FlippableClothingVisualsComponent : Component
{
    [DataField]
    public string FoldingLayer = "foldedLayer";

    [DataField]
    public string UnfoldingLayer = "unfoldedLayer";
}
