// SPDX-FileCopyrightText: 2026 AftrLite <61218133+AftrLite@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, Access(typeof(MalignRiftSpawnRule))]
public sealed partial class MalignRiftSpawnRuleComponent : Component
{
    [DataField] public EntProtoId MalignRift = "CosmicMalignRift";
    [DataField] public SoundSpecifier Tier2Sound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier2.ogg");
}
