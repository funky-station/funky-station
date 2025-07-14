using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Inventory;
using Content.Shared.Inventory;
using Content.Server.Power.Components;
using Content.Server.Radio.Components;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Speech;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Replays;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Server.Radio.EntitySystems;

public sealed class RadioSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly IReplayRecordingManager _replay = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ServerInventorySystem _inventorySystem = default!;

    // set used to prevent radio feedback loops.
    private readonly HashSet<string> _messages = new();

    private EntityQuery<TelecomExemptComponent> _exemptQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, RadioReceiveEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EntitySpokeEvent>(OnIntrinsicSpeak);

        _exemptQuery = GetEntityQuery<TelecomExemptComponent>();
    }

    private void OnIntrinsicSpeak(EntityUid uid, IntrinsicRadioTransmitterComponent component, EntitySpokeEvent args)
    {
        if (args.Channel != null && component.Channels.Contains(args.Channel.ID))
        {
            SendRadioMessage(uid, args.Message, args.Channel, uid);
            args.Channel = null; // prevent duplicate messages from other listeners.
        }
    }

    private void OnIntrinsicReceive(EntityUid uid, IntrinsicRadioReceiverComponent component, ref RadioReceiveEvent args)
    {
        if (args.Handled)
            return;
        if (TryComp(uid, out ActorComponent? actor))
        {
            _netMan.ServerSendMessage(args.ChatMsg, actor.PlayerSession.Channel);
            args.Handled = true;
        }
    }

    /// <summary>
    /// Send radio message to all active radio listeners
    /// </summary>
    public void SendRadioMessage(EntityUid messageSource, string message, ProtoId<RadioChannelPrototype> channel, EntityUid radioSource, bool escapeMarkup = true)
    {
        SendRadioMessage(messageSource, message, _prototype.Index(channel), radioSource, escapeMarkup: escapeMarkup);
    }

    /// <summary>
    /// Send radio message to all active radio listeners
    /// </summary>
    /// <param name="messageSource">Entity that spoke the message</param>
    /// <param name="radioSource">Entity that picked up the message and will send it, e.g. headset</param>
    public void SendRadioMessage(EntityUid messageSource, string message, RadioChannelPrototype channel, EntityUid radioSource, bool escapeMarkup = true)
    {
        // Midnight - Create a unique message identifier that includes both content and source
        var messageId = $"{message}_{messageSource}_{radioSource}";
        
        // TODO if radios ever garble / modify messages, feedback-prevention needs to be handled better than this.
        if (!_messages.Add(messageId))
            return;

        // Midnight - Play hidden radio sound locally around the sender if applicable with reduced range
        if (TryComp<HiddenRadioComponent>(radioSource, out var hiddenRadio) && 
            IsSoleProvider(messageSource, channel.ID, radioSource))
        {
            _audio.PlayPvs(hiddenRadio.Sound, messageSource, audioParams: AudioParams.Default.WithMaxDistance(7.5f).WithRolloffFactor(2f));
        }

        var evt = new TransformSpeakerNameEvent(messageSource, MetaData(messageSource).EntityName);
        RaiseLocalEvent(messageSource, evt);

        var name = evt.VoiceName;
        name = FormattedMessage.EscapeText(name);

        SpeechVerbPrototype speech;
        if (evt.SpeechVerb != null && _prototype.TryIndex(evt.SpeechVerb, out var evntProto))
            speech = evntProto;
        else
            speech = _chat.GetSpeechVerb(messageSource, message);

        var content = escapeMarkup
            ? FormattedMessage.EscapeText(message)
            : message;

        var wrappedMessage = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", content));

        // most radios are relayed to chat, so lets parse the chat message beforehand
        var chat = new ChatMessage(
            ChatChannel.Radio,
            message,
            wrappedMessage,
            NetEntity.Invalid,
            null);
        var chatMsg = new MsgChatMessage { Message = chat };
        var ev = new RadioReceiveEvent(message, messageSource, channel, radioSource, chatMsg);

        var sendAttemptEv = new RadioSendAttemptEvent(channel, radioSource);
        RaiseLocalEvent(ref sendAttemptEv);
        RaiseLocalEvent(radioSource, ref sendAttemptEv);
        var canSend = !sendAttemptEv.Cancelled;

        var sourceMapId = Transform(radioSource).MapID;
        var hasActiveServer = HasActiveServer(sourceMapId, channel.ID);
        var sourceServerExempt = _exemptQuery.HasComp(radioSource);

        // Midnight - Track which entities have already received this message to prevent duplicates
        // Group by the parent entity (the wearer) for headsets, or the entity itself for other radios
        var processedEntities = new HashSet<EntityUid>();

        var radioQuery = EntityQueryEnumerator<ActiveRadioComponent, TransformComponent>();
        while (canSend && radioQuery.MoveNext(out var receiver, out var radio, out var transform))
        {
            if (!radio.ReceiveAllChannels)
            {
                if (!radio.Channels.Contains(channel.ID) || (TryComp<IntercomComponent>(receiver, out var intercom) &&
                                                             !intercom.SupportedChannels.Contains(channel.ID)))
                    continue;
            }

            if (!channel.LongRange && transform.MapID != sourceMapId && !radio.GlobalReceive)
                continue;

            // don't need telecom server for long range channels or handheld radios and intercoms
            var needServer = !channel.LongRange && !sourceServerExempt;
            if (needServer && !hasActiveServer)
                continue;

            // check if message can be sent to specific receiver
            var attemptEv = new RadioReceiveAttemptEvent(channel, radioSource, receiver);
            RaiseLocalEvent(ref attemptEv);
            RaiseLocalEvent(receiver, ref attemptEv);
            if (attemptEv.Cancelled)
                continue;

            // Midnight - Determine the logical receiver entity
            EntityUid logicalReceiver;
            
            // For headsets, the logical receiver is the wearer
            if (TryComp<HeadsetComponent>(receiver, out var headset) && headset.IsEquipped)
            {
                logicalReceiver = Transform(receiver).ParentUid;
            }
            else
            {
                // For other radios (handheld, intrinsic, etc.), the logical receiver is the radio itself
                logicalReceiver = receiver;
            }

            // Skip if we've already processed this logical receiver
            if (!processedEntities.Add(logicalReceiver))
                continue;

            // send the message to the actual radio device
            RaiseLocalEvent(receiver, ref ev);
        }

        if (name != Name(messageSource))
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} as {name} on {channel.LocalizedName}: {message}");
        else
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} on {channel.LocalizedName}: {message}");

        _replay.RecordServerMessage(chat);
        _messages.Remove(messageId);
    }

    /// <inheritdoc cref="TelecomServerComponent"/>
    private bool HasActiveServer(MapId mapId, string channelId)
    {
        var servers = EntityQuery<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent, TransformComponent>();
        foreach (var (_, keys, power, transform) in servers)
        {
            if (transform.MapID == mapId &&
                power.Powered &&
                keys.Channels.Contains(channelId))
            {
                return true;
            }
        }
        return false;
    }
    
    // Midnight - MI13 Radio Jacket
    private bool IsSoleProvider(EntityUid user, string channelId, EntityUid radioSource)
    {
        // Get all headsets and radios the user has that can provide this channel
        var providers = GetAllRadioProviders(user, channelId).ToList();

        // Return true if:
        // 1. There's exactly one provider
        // 2. That provider is our hidden radio source
        return providers.Count == 1 && providers[0] == radioSource;
    }

    private IEnumerable<EntityUid> GetAllRadioProviders(EntityUid user, string channelId)
    {
        // Check equipment slots for headsets and active radios
        if (TryComp<InventoryComponent>(user, out var inventory))
        {
            foreach (var slot in inventory.Slots)
            {
                if (_inventorySystem.TryGetSlotEntity(user, slot.Name, out var item))
                {
                    // Check if it's a headset with the required slot and channel
                    if (TryComp<HeadsetComponent>(item, out var headset) && 
                        TryComp<EncryptionKeyHolderComponent>(item, out var headsetKeys) &&
                        headset.Enabled && headset.IsEquipped &&
                        headsetKeys.Channels.Contains(channelId))
                    {
                        yield return item.Value;
                    }
                    // Also check for active radios (handheld, etc.)
                    else if (TryComp<ActiveRadioComponent>(item, out var active) &&
                        TryComp<EncryptionKeyHolderComponent>(item, out var encryption) &&
                        encryption.Channels.Contains(channelId))
                    {
                        yield return item.Value;
                    }
                }
            }
        }

        // Check hands for handheld radios
        if (TryComp<HandsComponent>(user, out var hands))
        {
            foreach (var hand in hands.Hands.Values)
            {
                if (hand.HeldEntity != null)
                {
                    var item = hand.HeldEntity.Value;
                    if (TryComp<ActiveRadioComponent>(item, out var active) &&
                        TryComp<EncryptionKeyHolderComponent>(item, out var encryption) &&
                        encryption.Channels.Contains(channelId))
                    {
                        yield return item;
                    }
                }
            }
        }

        // Check the user themselves (for implanted radios)
        if (TryComp<ActiveRadioComponent>(user, out var userActive) &&
            TryComp<EncryptionKeyHolderComponent>(user, out var userEncryption) &&
            userEncryption.Channels.Contains(channelId))
        {
            yield return user;
        }
    }
}