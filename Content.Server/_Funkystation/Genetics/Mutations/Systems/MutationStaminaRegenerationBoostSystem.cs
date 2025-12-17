using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationStaminaRegenerationBoostSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (boost, stamina) in EntityQuery<MutationStaminaRegenerationBoostComponent, StaminaComponent>())
        {
            if (stamina.ActiveDrains.Count == 0)
            {
                _staminaSystem.TakeStaminaDamage(boost.Owner, -boost.RegenBonus * frameTime, stamina, visual: false);
            }
        }
    }
}
