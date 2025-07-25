// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Actions.Events;

/// <summary>
/// Adds / removes the component upon action.
/// </summary>
[Virtual]
public partial class ActionComponentChangeEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();
}

/// <summary>
/// Similar to <see cref="ActionComponentChangeEvent"/> except raises an event to attempt to relay it.
/// </summary>
public sealed partial class RelayedActionComponentChangeEvent : ActionComponentChangeEvent
{

}

[ByRefEvent]
public record struct AttemptRelayActionComponentChangeEvent
{
    public EntityUid? Target;
}
