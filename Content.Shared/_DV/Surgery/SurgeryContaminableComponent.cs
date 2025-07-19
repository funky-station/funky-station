// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Surgery;

/// <summary>
/// Component that indicates how an entity should respond to unsanitary surgery conditions
/// It also causes surgery tools to become dirty/cross contaminated when operated on.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))]
public sealed partial class SurgeryContaminableComponent : Component
{
    /// <summary>
    ///     How much cross contamination should increase dirtiness per incompatible DNA
    /// </summary>
    [DataField]
    public FixedPoint2 CrossContaminationDirtinessLevel = 60.0;

    /// <summary>
    ///     The level of dirtiness above which toxin damage will be dealt
    /// </summary>
    [DataField]
    public FixedPoint2 DirtinessThreshold = 75.0;

    /// <summary>
    /// The damage type to deal for sepsis.
    /// </summary>
    [DataField]
    public ProtoId<DamageTypePrototype> SepsisDamageType = "Poison";

    /// <summary>
    ///     The base amount of toxin damage to deal above the threshold
    /// </summary>
    [DataField]
    public FixedPoint2 BaseDamage = 0.50;

    /// <summary>
    ///     The inverse of the coefficient to scale the toxin damage by
    /// </summary>
    [DataField]
    public FixedPoint2 InverseDamageCoefficient = 250.0;

    /// <summary>
    ///     The upper limit on how much toxin damage can be dealt in a single step
    /// </summary>
    [DataField]
    public FixedPoint2 ToxinStepLimit = 15.0;
}
