// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Body.Part;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
///     Raised on the entity that is receiving surgery.
/// </summary>
[ByRefEvent]
public record struct SurgeryValidEvent(EntityUid Body, EntityUid Part, bool Cancelled = false, BodyPartType PartType = default, BodyPartSymmetry? Symmetry = default);