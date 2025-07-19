// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicMalignRiftComponent : Component
{
    [DataField]
    public bool Used;

    [DataField]
    public bool Occupied;

    [DataField]
    public EntProtoId PurgeVFX = "CleanseEffectVFX";

    [DataField]
    public SoundSpecifier PurgeSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/cleanse_deconversion.ogg");

    // [DataField]
    // public EntProtoId GrailID = "NullRodGrail"; // Not implemented at this time

    [DataField]
    public TimeSpan BibleTime = TimeSpan.FromSeconds(35);

    [DataField]
    public TimeSpan ChaplainTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan AbsorbTime = TimeSpan.FromSeconds(35);
}
