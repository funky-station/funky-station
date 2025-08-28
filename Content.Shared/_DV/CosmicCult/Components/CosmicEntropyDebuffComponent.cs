// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Makes the target take damage over time.
/// Meant to be used in conjunction with statusEffectSystem.
/// </summary>
[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicEntropyDebuffComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan CheckTimer = default!;

    [DataField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The debuff applied while the component is present.
    /// </summary>
    [DataField]
    public DamageSpecifier Degen = new()
    {
        DamageDict = new()
        {
            { "Cold", 0.25},
            { "Asphyxiation", 1.25},
        }
    };
}
