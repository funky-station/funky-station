// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicGlyphComponent : Component
{
    [DataField] public int RequiredCultists = 1;
    [DataField] public float ActivationRange = 1.55f;

    /// <summary>
    ///     Damage dealt on glyph activation.
    /// </summary>
    [DataField] public DamageSpecifier ActivationDamage = new();
    [DataField] public bool CanBeErased = true;
    [DataField] public EntProtoId GylphVFX = "CosmicGenericVFX";
    [DataField] public SoundSpecifier GylphSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_trigger.ogg");
}

public sealed class TryActivateGlyphEvent(EntityUid user, HashSet<Entity<CosmicCultComponent>> cultists) : CancellableEntityEventArgs
{
    public EntityUid User = user;
    public HashSet<Entity<CosmicCultComponent>> Cultists = cultists;
}
