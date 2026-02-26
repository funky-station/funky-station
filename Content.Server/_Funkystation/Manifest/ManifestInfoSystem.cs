using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.HealthExaminable;
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
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<MindComponent, TransferMindEvent>(OnMindTransfered);
        SubscribeLocalEvent<HealthExaminableComponent, RoundEndMessageEvent>(OnGod);
    }

    private void OnEntitySpoke(EntityUid uid, MobStateComponent _, EntitySpokeEvent args)
    {
        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
        {
            mind.LastMessage = args.Message;
        }
    }

    private void OnMindTransfered(EntityUid uid, MindComponent mind, TransferMindEvent args)
    {
        if (args.Target != null && !HasComp<GhostComponent>(args.Target))
        {
            mind.LastEntity = args.Target;
        }
    }

    private void OnGod(EntityUid uid, HealthExaminableComponent mind, RoundEndMessageEvent args)
    {
        if (true)
        {

        }
    }
}
