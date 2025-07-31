using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Radio;
using Content.Shared.Damage.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server.Damage.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SensitiveHearingSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandICChatRecipientsEvent);
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttemptEvent);
        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeechEvent);
        base.Initialize();
    }

    private void OnTransformSpeechEvent(TransformSpeechEvent args)
    {
        if (TryComp<SensitiveHearingComponent>(args.Sender, out var hearing) && hearing.IsDeaf)
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            args.Message = args.Message.ToUpper();
    }

    private void OnRadioReceiveAttemptEvent(ref RadioReceiveAttemptEvent ev)
    {
        //Check whether the entity that's receiving the radio message has an xform.
        var transform = CompOrNull<TransformComponent>(ev.RadioReceiver);
        var parentEntity = transform?.ParentUid;
        if (!parentEntity.HasValue || !HasComp<TransformComponent>(parentEntity))
            return;

        var hearing = CompOrNull<SensitiveHearingComponent>(parentEntity);
        if (hearing == null)
            return;

        //Cancel the event when the entity is deaf.
        ev.Cancelled = hearing.IsDeaf;
    }


    private void OnExpandICChatRecipientsEvent(ExpandICChatRecipientsEvent ev)
    {
        foreach (var recipient in ev.Recipients)
        {
            var entity = recipient.Key.AttachedEntity;
            var session = recipient.Key;
            if (!HasComp<SensitiveHearingComponent>(entity))
                continue;

            var hearing = CompOrNull<SensitiveHearingComponent>(entity);
            if (hearing is { IsDeaf: true })
            {
                    //Remove deaf recipients
                    ev.Recipients.Remove(session);

                    string message = Loc.GetString(
                        "damage-sensitive-hearing-deaf-message",
                        (
                            "user",
                            (entity == ev.Source) ? Loc.GetString("damage-sensitive-hearing-you") : Name(ev.Source)
                            ));
                    //"YOU said something" if source and recipient are the same, otherwise "SOMETHING/SOMEONE said something"
                    _popupSystem.PopupEntity(message, ev.Source, session, PopupType.Medium);
            }
        }

    }
}

