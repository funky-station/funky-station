// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Added to mind role entities to tag that they are the cosmic cult leader.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCosmicCultSystem))]
public sealed partial class CosmicCultLeadComponent : Component
{
    public override bool SessionSpecific => true;

    /// <summary>
    /// The status icon corresponding to the lead cultist.
    /// </summary>
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "CosmicCultLeadIcon";

    /// <summary>
    /// How long the stun will last after the user is converted.
    /// </summary>
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(3);

    [DataField]
    public EntProtoId MonumentPrototype = "MonumentCosmicCultSpawnIn";

    [DataField]
    public EntProtoId CosmicMonumentPlaceAction = "ActionCosmicPlaceMonument";

    [DataField]
    public EntityUid? CosmicMonumentPlaceActionEntity;

    [DataField]
    public EntProtoId CosmicMonumentMoveAction = "ActionCosmicMoveMonument";

    [DataField]
    public EntityUid? CosmicMonumentMoveActionEntity;
}
