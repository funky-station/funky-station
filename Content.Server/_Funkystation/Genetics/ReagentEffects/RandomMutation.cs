using Content.Server._Funkystation.Genetics.Components;
using Content.Server._Funkystation.Genetics.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Funkystation.Genetics.ReagentEffects;

public sealed partial class RandomMutation : EntityEffect
{
    /// <summary>
    /// Chance (0.0–1.0) that a random mutation is triggered each time the effect runs.
    /// </summary>
    [DataField]
    public float Chance = 1.0f;

    /// <summary>
    /// Minimum number of mutations to attempt (will be rolled between Min and Max).
    /// </summary>
    [DataField]
    public int MinMutations = 1;

    /// <summary>
    /// Maximum number of mutations to attempt.
    /// </summary>
    [DataField]
    public int MaxMutations = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutation", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args.TargetEntity is not { Valid: true } entity)
            return;

        var entMan = args.EntityManager;

        if (!entMan.HasComponent<GeneticsComponent>(entity))
            return;

        if (!entMan.TryGetComponent<GeneticsComponent>(entity, out var genetics))
            return;

        var geneticsSystem = entMan.System<GeneticsSystem>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var scale = args is EntityEffectReagentArgs reagentArgs ? reagentArgs.Scale.Float() : 1f;
        var attempts = random.Next(MinMutations, MaxMutations + 1);

        var mutationsApplied = 0;

        for (int i = 0; i < attempts; i++)
        {
            if (random.Prob(Chance * scale))
            {
                geneticsSystem.TriggerRandomMutation(entity, genetics);
                mutationsApplied++;
            }
        }
    }
}
