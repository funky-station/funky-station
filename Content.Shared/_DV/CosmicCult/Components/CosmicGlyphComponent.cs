// SPDX-FileCopyrightText: 2025 No Elka <125199100+NoElkaTheGod@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicGlyphComponent : Component
{
    [DataField] public int RequiredCultists = 1;
    [DataField] public float ActivationRange = 1.55f;

    /// <summary>
    ///     Damage dealt on glyph activation.
    /// </summary>
    [DataField] public DamageSpecifier ActivationDamage = new();
    [DataField] public bool CanBeErased = true;
    [DataField] public bool EraseOnUse = false;
    [DataField] public EntProtoId GlyphVFX = "CosmicGenericVFX";
    [DataField] public SoundSpecifier TriggerSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_trigger.ogg");
    [DataField] public GlyphStatus State = GlyphStatus.Spawning;
    [DataField] public EntityUid? User = null;
    [DataField] public TimeSpan SpawnTime = TimeSpan.FromSeconds(1.2);
    [DataField] public TimeSpan DespawnTime = TimeSpan.FromSeconds(0.6);
    [DataField] public TimeSpan CooldownTime = TimeSpan.FromSeconds(3.0);
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Timer = default!;
}

[ByRefEvent]
public record struct TryActivateGlyphEvent(EntityUid User, HashSet<Entity<CosmicCultComponent>> Cultists, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}

[Serializable, NetSerializable]
public enum GlyphVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum GlyphStatus : byte
{
    Spawning,
    Despawning,
    Ready,
    Cooldown
}
