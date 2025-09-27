// SPDX-FileCopyrightText: 2025 8tv <eightev@gmail.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Chat.TypingIndicator;

[Serializable, NetSerializable]
public enum TypingIndicatorState
{
    None = 0,
    Idle = 1,
    Typing = 2,
}
