using Content.Shared.BloodCult;
using Content.Shared.EntityEffects;

namespace Content.Shared._Funkystation.BloodCult;

public sealed class DeCultifySystem : EntityEffectSystem<BloodCultistComponent, DeCultify>
{
    protected override void Effect(Entity<BloodCultistComponent> entity, ref EntityEffectEvent<DeCultify> args)
    {
        entity.Comp.DeCultification += args.Effect.Amount * args.Scale;
    }
}
