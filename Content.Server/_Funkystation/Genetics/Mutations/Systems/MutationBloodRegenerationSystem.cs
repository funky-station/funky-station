using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationBloodRegenerationSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    private float _accum = 0f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accum += frameTime;
        if (_accum < 1.0f)
            return;

        _accum -= 1.0f;

        var query = EntityQueryEnumerator<MutationBloodRegenerationComponent, BloodstreamComponent>();
        while (query.MoveNext(out var uid, out var regen, out var bloodstream))
        {
            // Skip if dead or in crit (no passive regen while dead)
            if (_mobState.IsDead(uid) || _mobState.IsCritical(uid))
                continue;

            var currentPercentage = _bloodstream.GetBloodLevelPercentage(uid, bloodstream);

            if (currentPercentage >= regen.TargetPercentage)
                continue; // Already at or above target

            if (!_solutionContainerSystem.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution))
                continue;

            var deficitPercentage = regen.TargetPercentage - currentPercentage;
            var maxVolume = bloodSolution.MaxVolume.Float();
            var regenThisTick = MathF.Min(regen.RegenRatePerSecond, deficitPercentage * maxVolume);

            _bloodstream.TryModifyBloodLevel(uid, regenThisTick, bloodstream);
        }
    }
}
