using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Damage.Components;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationIncreaseStaminaCritThresholdSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationIncreaseStaminaCritThresholdComponent, ComponentAdd>(OnAdd);
        SubscribeLocalEvent<MutationIncreaseStaminaCritThresholdComponent, ComponentRemove>(OnRemove);
    }

    private void OnAdd(EntityUid uid, MutationIncreaseStaminaCritThresholdComponent comp, ComponentAdd args)
    {
        if (TryComp<StaminaComponent>(uid, out var stamina))
        {
            stamina.CritThreshold += comp.ThresholdBonus;
            Dirty(uid, stamina);
        }
    }

    private void OnRemove(EntityUid uid, MutationIncreaseStaminaCritThresholdComponent comp, ComponentRemove args)
    {
        if (TryComp<StaminaComponent>(uid, out var stamina))
        {
            stamina.CritThreshold -= comp.ThresholdBonus;
            stamina.CritThreshold = Math.Max(100f, stamina.CritThreshold);
            Dirty(uid, stamina);
        }
    }
}
