using Content.Server._Funkystation.Genetics.Components;
using Robust.Shared.Timing;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class GeneticsInstabilityDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const int InstabilityThreshold = 150;
    private const float DamagePerTick = 1f;
    private const float TickInterval = 2f;

    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < TickInterval)
            return;

        _accumulator -= TickInterval;

        var query = EntityQueryEnumerator<GeneticsComponent, DamageableComponent, GeneticsInstabilityDamageComponent>();
        while (query.MoveNext(out var uid, out var genetics, out var damageable, out _))
        {
            if (genetics.GeneticInstability <= InstabilityThreshold)
                continue;

            var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Cellular"), DamagePerTick);
            _damageable.TryChangeDamage(uid, damage, true, damageable: damageable);
        }
    }
}

[RegisterComponent]
public sealed partial class GeneticsInstabilityDamageComponent : Component { }
