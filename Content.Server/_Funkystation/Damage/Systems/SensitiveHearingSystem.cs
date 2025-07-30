using Content.Server.Chat.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Damage.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SensitiveHearingSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandICChatRecipientsEvent);
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpokeEvent);
        base.Initialize();
    }

    private void OnEntitySpokeEvent(EntitySpokeEvent ev)
    {
        Log.Warning($"{ev}");
    }

    private void OnExpandICChatRecipientsEvent(ExpandICChatRecipientsEvent ev)
    {
        if (!HasComp<SensitiveHearingComponent>(ev.Source))
            return;

        var hearing = Comp<SensitiveHearingComponent>(ev.Source);
        // hearing.damageAmount

        var sourceSession = GetEntityICommonSession(ev.Source);
        if (sourceSession != null)
            ev.Recipients.Remove(sourceSession);
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

