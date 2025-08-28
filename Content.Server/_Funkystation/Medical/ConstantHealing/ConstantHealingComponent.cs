// SPDX-FileCopyrightText: 2025 Patrycja <git@ptrcnull.me>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

[RegisterComponent]
public sealed partial class ConstantHealingComponent : Component
{
    /// <summary>
    /// Effects to apply every cycle.
    /// </summary>
    [DataField("effects", required: true)]
    public List<EntityEffect> Effects = default!;
    /// <summary>
    ///     The next time that reagents will be metabolized.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;

    /// <summary>
    ///     How often to metabolize reagents.
    /// </summary>
    /// <returns></returns>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}
