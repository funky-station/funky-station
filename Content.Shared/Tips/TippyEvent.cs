// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT


using Robust.Shared.Serialization;

namespace Content.Shared.Tips;

[Serializable, NetSerializable]
public sealed class TippyEvent : EntityEventArgs
{
    public TippyEvent(string msg)
    {
        Msg = msg;
    }

    public string Msg;
    public string? Proto;
    public float SpeakTime = 5;
    public float SlideTime = 3;
    public float WaddleInterval = 0.5f;
}
