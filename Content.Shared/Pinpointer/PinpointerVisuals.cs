// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Pinpointer
{
    [Serializable, NetSerializable]
    public enum PinpointerVisuals : byte
    {
        IsActive,
        ArrowAngle,
        TargetDistance
    }

    public enum PinpointerLayers : byte
    {
        Base,
        Screen
    }
}
