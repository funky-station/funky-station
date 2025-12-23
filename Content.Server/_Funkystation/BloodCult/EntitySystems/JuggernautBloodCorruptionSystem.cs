// Content.Server.BloodCult.EntityEffects.Systems

using Content.Server.Fluids.EntitySystems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.FixedPoint;


namespace Content.Server.BloodCult.EntityEffects.Systems;

public sealed class JuggernautBloodCorruptionSystem : EntitySystem
{
    [Dependency] private readonly PuddleSystem _puddle = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ReactiveComponent, ReactionEntityEvent>(OnReaction);
    }

    private void OnReaction(Entity<ReactiveComponent> entity, ref ReactionEntityEvent args)
    {
        var uid = entity.Owner;

        // Only juggernauts
        if (!HasComp<JuggernautComponent>(uid))
            return;

        var reagentId = args.Reagent.ID;

        // Ignore already corrupted or invalid blood
        if (reagentId == "SanguinePerniculate" ||
            !BloodCultConstants.SacrificeBloodReagents.Contains(reagentId))
            return;

        var quantity = args.ReagentQuantity.Quantity;
        if (quantity <= FixedPoint2.Zero)
            return;

        var solution = new Solution();
        solution.AddReagent("SanguinePerniculate", quantity);

        _puddle.TrySpillAt(
            Transform(uid).Coordinates,
            solution,
            out _,
            sound: false);
    }
}
