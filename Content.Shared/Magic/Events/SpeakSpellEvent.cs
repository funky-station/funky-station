// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Magic.Events;

[ByRefEvent]
public readonly struct SpeakSpellEvent(EntityUid performer, string speech)
{
    public readonly EntityUid Performer = performer;
    public readonly string Speech = speech;
}
