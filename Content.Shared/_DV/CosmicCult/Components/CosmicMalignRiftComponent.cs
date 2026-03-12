// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicMalignRiftComponent : Component
{
    public DoAfterId? DoAfterId = null;

    [DataField] public bool Used;

    [DataField] public bool Occupied;

    [DataField] public TimeSpan AbsorbTime = TimeSpan.FromSeconds(30);
    [DataField] public TimeSpan PurgeTime = TimeSpan.FromSeconds(25);
    [DataField] public EntProtoId PurgeVFX = "CleanseEffectVFX";
    [DataField] public ProtoId<HitscanPrototype> BeamVFX = "CosmicLambdaBeam";
    [DataField] public SoundSpecifier PurgeSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/effigy_pulse.ogg");
    [DataField] public SoundSpecifier BeamSFX = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/laser_cannon2.ogg");
}
