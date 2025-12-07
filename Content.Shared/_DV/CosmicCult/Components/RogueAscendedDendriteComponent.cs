// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Ascendant's Dendrite for the reward system.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RogueAscendedDendriteComponent : Component
{
    [DataField] public SoundSpecifier ActivateSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_nova_impact.ogg");
    [DataField] public EntProtoId Vfx = "CosmicGenericVFX";
    [DataField, AutoNetworkedField] public TimeSpan StunTime = TimeSpan.FromSeconds(2);
    [DataField] public EntProtoId RogueFoodAction = "ActionRogueCosmicNova";
    [DataField] public EntityUid? RogueFoodActionEntity;
}
