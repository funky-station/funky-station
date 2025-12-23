using Content.Server.Fluids.EntitySystems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.FixedPoint;

namespace Content.Server.BloodCult.Chemistry;

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

        // Only sacrifice blood, ignore already-corrupted blood
        if (!BloodCultConstants.SacrificeBloodReagents.Contains(reagentId) ||
            reagentId == "SanguinePerniculate")
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
