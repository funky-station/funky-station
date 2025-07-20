// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Speech;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;

namespace Content.Shared.Chat;

/// <summary>
///     This event should be sent everytime an entity talks (Radio, local chat, etc...).
///     The event is sent to both the entity itself, and all clothing (For stuff like voice masks).
/// </summary>
public sealed class TransformSpeakerNameEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;
    public EntityUid Sender;
    public string VoiceName;
    public ProtoId<SpeechVerbPrototype>? SpeechVerb;

    public TransformSpeakerNameEvent(EntityUid sender, string name)
    {
        Sender = sender;
        VoiceName = name;
        SpeechVerb = null;
    }
}
