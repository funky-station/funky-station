using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.Popups;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed class InstabilityCountdownSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PendingInstabilityMutationComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, PendingInstabilityMutationComponent pending, ComponentStartup args)
    {
        pending.StartTime = _timing.CurTime;
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        foreach (var pending in EntityQuery<PendingInstabilityMutationComponent>())
        {
            var uid = pending.Owner;
            var remaining = pending.EndTime - curTime;

            if (remaining <= TimeSpan.Zero)
            {
                TriggerMutation(uid, pending);
                RemComp<PendingInstabilityMutationComponent>(uid);
                continue;
            }

            var totalDuration = pending.EndTime - pending.StartTime;

            if (remaining <= TimeSpan.FromSeconds(10) && !pending.Warning10Sec)
            {
                _popup.PopupEntity(Loc.GetString("genetics-instability-warning-10sec"), uid, uid);
                pending.Warning10Sec = true;
            }

            if (remaining <= totalDuration / 2 && !pending.WarningHalfway)
            {
                _popup.PopupEntity(Loc.GetString("genetics-instability-warning-half"), uid, uid);
                pending.WarningHalfway = true;
            }

            if (remaining <= totalDuration - TimeSpan.FromSeconds(10) && !pending.WarningStart)
            {
                _popup.PopupEntity(Loc.GetString("genetics-instability-warning-start"), uid, uid);
                pending.WarningStart = true;
            }
        }
    }

    private void TriggerMutation(EntityUid uid, PendingInstabilityMutationComponent pending)
    {
        if (!TryComp<GeneticsComponent>(uid, out var genetics))
            return;

        // Pick a random instability mutation
        var validProtos = new List<GeneticMutationPrototype>();
        foreach (var proto in _proto.EnumeratePrototypes<GeneticMutationPrototype>())
        {
            if (!proto.InstabilityMutation)
                continue;
            if (!_genetics.CanEntityReceiveMutation(uid, proto, false))
                continue;
            if (genetics.Mutations.Any(m => m.Id == proto.ID && m.Enabled))
                continue;
            validProtos.Add(proto);
        }

        if (validProtos.Count == 0)
            return;

        var chosenProto = _random.Pick(validProtos);

        // Deactivate conflicts
        foreach (var conflictId in chosenProto.Conflicts)
        {
            var conflictEntry = genetics.Mutations.FirstOrDefault(m => m.Id == conflictId);
            if (conflictEntry != null && conflictEntry.Enabled)
                _genetics.TryDeactivateMutation(uid, genetics, conflictId);
        }

        // Add mutation
        if (!genetics.Mutations.Any(m => m.Id == chosenProto.ID))
            _genetics.TryAddMutation(uid, genetics, chosenProto.ID);

        // Activate
        _genetics.TryActivateMutation(uid, genetics, chosenProto.ID);
    }
}
