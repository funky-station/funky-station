// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// This is used to mark an entity as the end point for the "relocate monument" ability. ideally there should only ever be one of these
/// </summary>
[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class MonumentMoveDestinationComponent : Component
{
    public EntityUid? Monument;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? PhaseInTimer;
}
