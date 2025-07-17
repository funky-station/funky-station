// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;

namespace Content.Shared.Actions.Events;

[ByRefEvent]
public record struct ValidateActionEntityWorldTargetEvent(
    EntityUid User,
    EntityUid? Target,
    EntityCoordinates? Coords,
    bool Cancelled = false);
