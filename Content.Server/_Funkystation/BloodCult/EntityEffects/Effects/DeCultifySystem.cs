using Content.Shared.EntityEffects;
using Content.Shared.BloodCult;

namespace Content.Server.EntityEffects.Effects;

public sealed class DeCultifySystem : EntityEffectSystem<BloodCultistComponent, DeCultify>
{
    protected override void Effect(Entity<BloodCultistComponent> entity, ref EntityEffectEvent<DeCultify> args)
    {
        entity.Comp.DeCultification += args.Effect.Amount * args.Scale;
    }
}
