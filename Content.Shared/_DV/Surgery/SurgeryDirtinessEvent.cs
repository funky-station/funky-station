// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Surgery;

/// <summary>
/// 	Handled by the server when a surgery step is completed in order to deal with sanitization concerns
/// </summary>
[ByRefEvent]
public record struct SurgeryDirtinessEvent(EntityUid User, EntityUid Part, List<EntityUid> Tools, EntityUid Step);
