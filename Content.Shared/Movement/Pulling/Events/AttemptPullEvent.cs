// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Physics.Components;

namespace Content.Shared.Movement.Pulling.Events;

/// <summary>
/// Raised directed on puller and pullable to determine if it can be pulled.
/// </summary>
public sealed class PullAttemptEvent : PullMessage
{
    public PullAttemptEvent(EntityUid pullerUid, EntityUid pullableUid) : base(pullerUid, pullableUid) { }

    public bool Cancelled { get; set; }
}
