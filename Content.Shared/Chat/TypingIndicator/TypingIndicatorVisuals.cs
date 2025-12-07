// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2025 88tv <131759102+88tv@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tojo <32783144+Alecksohs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Chat.TypingIndicator;

// FUNKYSTATION EDIT - ADD CHATTYPE AND OVERRIDEPROTOTYPE
[Serializable, NetSerializable]
public enum TypingIndicatorVisuals : byte
{
    State,
    ChatType,
    OverrideIndicatorPrototype
}

[Serializable]
public enum TypingIndicatorLayers : byte
{
    Base,
}
