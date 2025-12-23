using Content.Shared.BloodCult;
using Content.Shared.BloodCult.EntityEffects;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;

public sealed class BleedSanguinePerniculateSystem
    : EntityEffectSystem<BloodstreamComponent, BleedSanguinePerniculateEffect>
{
    protected override void Effect(
        Entity<BloodstreamComponent> entity,
        ref EntityEffectEvent<BleedSanguinePerniculateEffect> args)
    {
        var uid = entity.Owner;
        var bloodstream = entity.Comp;

        var edge = EnsureComp<EdgeEssentiaBloodComponent>(uid);

        if (!edge.Active)
        {
            var sanguine = new Solution();
            sanguine.AddReagent("SanguinePerniculate", FixedPoint2.New(1));

            edge.AppliedBloodOverride = sanguine;
            edge.Active = true;
            Dirty(uid, edge);
        }

        EntitySystem.Get<SharedBloodstreamSystem>()
            .ChangeBloodReagents((uid, bloodstream), edge.AppliedBloodOverride!);
    }
}
