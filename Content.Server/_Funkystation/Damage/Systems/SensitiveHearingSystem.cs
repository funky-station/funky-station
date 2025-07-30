using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Damage.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Damage.Systems;

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
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpokeEvent);
        base.Initialize();
    }

    private void OnExpandICRecipientsEvent(ExpandICEvent ev)
    {
        throw new NotImplementedException();
    }

    private void OnEntitySpokeEvent(EntitySpokeEvent ev)
    {

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
                    ev.Recipients.Remove(session);
                    _popupSystem.PopupEntity("u can't hear bozo", ev.Source, session, PopupType.Medium);
            }
        }

    }

    private ICommonSession? GetEntityICommonSession(EntityUid entity)
    {
        MindContainerComponent? mindContainer = CompOrNull<MindContainerComponent>(entity);
        MindComponent? mind;
        if (mindContainer == null || !mindContainer.HasMind)
            return null;
        mind = CompOrNull<MindComponent>(mindContainer.Mind);
        return mind?.Session;
    }


}

