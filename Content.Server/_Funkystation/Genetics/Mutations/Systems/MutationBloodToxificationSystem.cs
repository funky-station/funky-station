using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationBloodToxificationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationBloodToxificationComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, MutationBloodToxificationComponent comp, ComponentInit args)
    {
        comp.NextTick = _timing.CurTime + TimeSpan.FromSeconds(comp.Interval);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutationBloodToxificationComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var comp, out var damageable))
        {
            if (_timing.CurTime < comp.NextTick)
                continue;

            comp.NextTick += TimeSpan.FromSeconds(comp.Interval);

            if (!_random.Prob(comp.Chance))
                continue;

            var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Poison"), comp.ToxinAmount);
            _damageable.TryChangeDamage(uid, damage, true, damageable: damageable);
        }
    }
}
