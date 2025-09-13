// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;

namespace Content.Shared.MalfAI;

/// <summary>
/// Raised when the Malf AI Doomsday Protocol countdown completes (was not aborted).
/// Systems can handle this to implement the actual doomsday effect.
/// </summary>
public sealed class MalfAiDoomsdayCompletedEvent : EntityEventArgs
{
    public EntityUid Station { get; }
    public EntityUid Ai { get; }

    public MalfAiDoomsdayCompletedEvent(EntityUid station, EntityUid ai)
    {
        Station = station;
        Ai = ai;
    }
}
