using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.EntityEffects.Effects;
using Content.Shared.EntityEffects;

namespace Content.Server.BloodCult.EntityEffects.Systems;

/// <summary>
/// Handles DeleteEntityEffect execution.
/// </summary>
public sealed class DeleteEntityEffectSystem
    : EntityEffectSystem<CleanableRuneComponent, DeleteEntityEffect>
{
    protected override void Effect(
        Entity<CleanableRuneComponent> entity,
        ref EntityEffectEvent<DeleteEntityEffect> args)
    {
        var uid = entity.Owner;

        // Do not delete special runes
        if (HasComp<TearVeilComponent>(uid) ||
            HasComp<FinalSummoningRuneComponent>(uid))
        {
            return;
        }

        QueueDel(uid);
    }
}
