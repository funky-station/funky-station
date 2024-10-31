using System.Linq;
using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Utility;
using Content.Shared.Item;
using Content.Shared.Revolutionary;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Construction.Conditions;

[UsedImplicitly]
[DataDefinition]
/// <summary>
/// A graph condition to see if a crafter has a certain component. Useful if you want certain
/// recipes only accessible by "knowledge" or some other mechanic. Only works if the item
/// is parented to their character, which, I know, drawback.
/// </summary>
public sealed partial class RevolutionaryCrafterHasRecipeComponent : IGraphCondition
{

    [DataField]
    public string RecipeName;

    [DataField]
    public string? GuideText { get; private set; }
    [DataField]
    public SpriteSpecifier? GuideIcon { get; private set; }

    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        var minds = entityManager.System<SharedMindSystem>();
        var transformComponent = entityManager.GetComponentOrNull<TransformComponent>(uid);

        if (transformComponent == null)
            return false;

        if (!minds.TryGetMind(transformComponent.ParentUid, out var mindId, out var mindComp))
            return false;

        if (mindComp.CurrentEntity == null)
            return false;

        var revComp = entityManager.GetComponent<HeadRevolutionaryPathComponent>((EntityUid) mindComp.CurrentEntity);

        return revComp.Recipes.Any(recipe => recipe == RecipeName);
    }

    public bool DoExamine(ExaminedEvent args)
    {
        var entity = args.Examined;

        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!entMan.TryGetComponent(entity, out ItemComponent? item)) return false;

        args.PushMarkup("Something might happen if you use a screwdriver on it.");

        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        if (GuideText == null)
                yield break;

        yield return new ConstructionGuideEntry()
        {
            Localization = GuideText,
            Icon = GuideIcon,
        };
    }
}

