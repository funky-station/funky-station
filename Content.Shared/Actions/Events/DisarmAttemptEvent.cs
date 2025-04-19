// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Actions.Events;

/// <summary>
/// Raised directed on the target OR their actively held entity.
/// </summary>
[ByRefEvent]
public record struct DisarmAttemptEvent
{
    public readonly EntityUid TargetUid;
    public readonly EntityUid DisarmerUid;
    public readonly EntityUid? TargetItemInHandUid;

    public bool Cancelled;

    public DisarmAttemptEvent(EntityUid targetUid, EntityUid disarmerUid, EntityUid? targetItemInHandUid = null)
    {
        TargetUid = targetUid;
        DisarmerUid = disarmerUid;
        TargetItemInHandUid = targetItemInHandUid;
    }
}
