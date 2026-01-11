using Content.Server._Funkystation.Genetics.Mutations.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Damage.Components;
using Content.Server.Damage.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationStupefactionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationStupefactionComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, MutationStupefactionComponent comp, ComponentInit args)
    {
        ScheduleNextDrain(comp);
    }

    private void ScheduleNextDrain(MutationStupefactionComponent comp)
    {
        var delay = _random.NextFloat(comp.MinInterval, comp.MaxInterval);
        comp.NextDrainTime = _timing.CurTime + TimeSpan.FromSeconds(delay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutationStupefactionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextDrainTime)
                continue;

            // Only affect entities with stamina
            if (!TryComp<StaminaComponent>(uid, out var stamina))
            {
                ScheduleNextDrain(comp);
                continue;
            }

            _stamina.TakeStaminaDamage(uid, comp.DrainAmount, stamina);

            ScheduleNextDrain(comp);
        }
    }
}
