using Content.Shared.EntityEffects;
using Content.Shared.BloodCult;

namespace Content.Server.EntityEffects.Effects;

public sealed class DeCultifySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeCultify>(OnDeCultify);
    }

    private void OnDeCultify(EntityUid uid, EntityEffectEvent<DeCultify> args)
    {
        if (!TryComp<BloodCultistComponent>(uid, out var bloodCultist))
            return;

        bloodCultist.DeCultification += args.Effect.Amount * args.Scale;
    }
}
