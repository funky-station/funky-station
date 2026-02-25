using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Shared._Funkystation.Manifest;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Manifest;

public sealed class LastWordsSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<MindComponent, TransferMindEvent>(OnMindTransfered);
        _net.RegisterNetMessage<DeathInformationMessage>(OnDeathInfoFeedback);
    }

    private void OnEntitySpoke(EntityUid uid, MobStateComponent _, EntitySpokeEvent args)
    {
        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
        {
            mind.LastMessage = args.Message;
        }
    }

    //Handles when the mind changes entity
    //If its a ghost, prompt the user (they died)
    //if not, update LastEntity.
    private void OnMindTransfered(EntityUid uid, MindComponent _, TransferMindEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind))
            return;
        //if the target is not a ghost ...
        if (!TryComp<GhostComponent>(args.Target, out var _))
        {
            mind.LastEntity = args.Target;
            mind.DeathInfo = null;
            return;
        }
        else
        {
            if (!_mindSystem.TryGetSession(uid, out var session))
                return;
            RaiseNetworkEvent(new DeathInfoOpenMessage(), session);
        }
    }

    //Collect text from what the user inputs
    private void OnDeathInfoFeedback(DeathInformationMessage msg)
    {
        if (!_mindSystem.TryGetMind(msg.MsgChannel.UserId, out var mind))
            return;
        if (!TryComp<MindComponent>(mind, out var comp))
            return;
        comp.DeathInfo = msg.Description;
    }
}
