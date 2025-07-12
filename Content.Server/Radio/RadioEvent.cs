using Content.Shared.Chat;
using Content.Shared.Radio;

namespace Content.Server.Radio;

[ByRefEvent]
public struct RadioReceiveEvent
{
    public readonly string Message;
    public readonly EntityUid MessageSource;
    public readonly RadioChannelPrototype Channel;
    public readonly EntityUid RadioSource;
    public readonly MsgChatMessage ChatMsg;
    public bool Handled;

    public RadioReceiveEvent(string message, EntityUid messageSource, RadioChannelPrototype channel, EntityUid radioSource, MsgChatMessage chatMsg)
    {
        Message = message;
        MessageSource = messageSource;
        Channel = channel;
        RadioSource = radioSource;
        ChatMsg = chatMsg;
        Handled = false;
    }
}

/// <summary>
/// Use this event to cancel sending message per receiver
/// </summary>
[ByRefEvent]
public record struct RadioReceiveAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource, EntityUid RadioReceiver)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public readonly EntityUid RadioReceiver = RadioReceiver;
    public bool Cancelled = false;
}

/// <summary>
/// Use this event to cancel sending message to every receiver
/// </summary>
[ByRefEvent]
public record struct RadioSendAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public bool Cancelled = false;
}
