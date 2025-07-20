// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Raised on the step entity and the user after doing a step.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepEvent(EntityUid User, EntityUid Body, EntityUid Part, List<EntityUid> Tools, EntityUid Surgery, EntityUid Step, bool Complete);

/// <summary>
/// Raised on the user after failing to do a step for any reason.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepFailedEvent(EntityUid User, EntityUid Body, EntProtoId SurgeryId, EntProtoId StepId);
