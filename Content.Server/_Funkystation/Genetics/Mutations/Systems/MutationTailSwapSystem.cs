using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationTailSwapSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationTailSwapComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MutationTailSwapComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid ent, MutationTailSwapComponent comp, ref ComponentStartup args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
            return;

        // Save all original tail markings and colors
        var originalTailMarkings = new List<(string, List<Color>)>();
        if (humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.Tail, out var currentTails))
        {
            foreach (var marking in currentTails)
            {
                var colors = new List<Color>();
                for (var i = 0; i < marking.MarkingColors.Count; i++)
                {
                    colors.Add(marking.MarkingColors[i]);
                }
                originalTailMarkings.Add((marking.MarkingId, colors));
            }
        }
        comp.OriginalTailMarkings = originalTailMarkings;

        // Remove all existing tails
        humanoid.MarkingSet.RemoveCategory(MarkingCategories.Tail);

        // Determine the color to use for the new tail
        Color tailColor;
        if (comp.TailColor is { } customColor)
            tailColor = customColor;
        else
            tailColor = humanoid.SkinColor;

        if (_proto.TryIndex<MarkingPrototype>(comp.NewTailMarking, out var markingProto))
        {
            var spriteCount = markingProto.Sprites.Count;
            var colors = new List<Color>();

            for (var i = 0; i < spriteCount; i++)
            {
                colors.Add(tailColor);
            }

            // Add the new tail with the chosen color
            _humanoid.AddMarking(ent, comp.NewTailMarking, colors, forced: true);
        }
        else
        {
            // just add without color if prototype not found (should never happen)
            _humanoid.AddMarking(ent, comp.NewTailMarking, forced: true);
        }

        Dirty(ent, humanoid);
    }

    private void OnShutdown(EntityUid ent, MutationTailSwapComponent comp, ref ComponentShutdown args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
            return;

        humanoid.MarkingSet.Remove(MarkingCategories.Tail, comp.NewTailMarking);

        if (comp.OriginalTailMarkings is { } originals)
        {
            foreach (var (markingId, colors) in originals)
            {
                _humanoid.AddMarking(ent, markingId, colors, forced: true);
            }
        }

        Dirty(ent, humanoid);
    }
}
