using Content.Server._Funkystation.Genetics.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server._Funkystation.Genetics.Components;

namespace Content.Server._Funkystation.Genetics.ReagentEffects;

public sealed partial class MutationRemoval : EntityEffect
{
    [DataField] public float Chance = 1.0f;
    [DataField] public int MinRemovals = 1;
    [DataField] public int MaxRemovals = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutation-removal", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args.TargetEntity is not { Valid: true } entity)
            return;

        var entMan = args.EntityManager;

        if (!entMan.HasComponent<GeneticsComponent>(entity))
            return;

        if (!entMan.TryGetComponent(entity, out GeneticsComponent? genetics) || genetics == null)
            return;

        var geneticsSystem = entMan.System<GeneticsSystem>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var scale = args is EntityEffectReagentArgs reagentArgs ? reagentArgs.Scale.Float() : 1f;
        var attempts = random.Next(MinRemovals, MaxRemovals + 1);

        var removalsApplied = 0;

        for (int i = 0; i < attempts; i++)
        {
            if (random.Prob(Chance * scale))
            {
                geneticsSystem.RemoveRandomMutation(entity, genetics, true);
                removalsApplied++;
            }
        }
    }
}
