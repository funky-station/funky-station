// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Visuals;

[Serializable, NetSerializable]
public enum PipeScrubberVisuals : byte
{
    IsFull,
    IsEnabled,
    IsScrubbing,
    IsDraining,
}
