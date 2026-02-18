using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationParanoiaSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationParanoiaComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, MutationParanoiaComponent comp, ComponentInit args)
    {
        comp.NextCheck = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(0f, comp.Interval));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<MutationParanoiaComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextCheck) continue;

            comp.NextCheck = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(0.8f * comp.Interval, 1.2f * comp.Interval));

            if (_random.Prob(comp.EmoteChance))
                _chat.TryEmoteWithChat(uid, "Scream");
        }
    }
}
