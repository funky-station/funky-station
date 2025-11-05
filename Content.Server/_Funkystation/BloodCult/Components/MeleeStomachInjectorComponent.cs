// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.FixedPoint;

namespace Content.Server.BloodCult.Components;

/// <summary>
/// Like MeleeChemicalInjector, but injects into the target's stomach instead of bloodstream.
/// Used by cult daggers to inject EdgeEssentia into the stomach where it metabolizes
/// without spilling into blood puddles.
/// </summary>
[RegisterComponent]
public sealed partial class MeleeStomachInjectorComponent : Component
{
    /// <summary>
    /// How much solution to inject per hit.
    /// </summary>
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(3);

    /// <summary>
    /// Solution to inject from.
    /// </summary>
    [DataField]
    public string Solution = "melee";
}

