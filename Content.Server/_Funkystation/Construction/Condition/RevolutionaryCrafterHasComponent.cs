// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Utility;
using Content.Shared.Item;
using Content.Shared.Revolutionary.Components;

namespace Content.Server.Construction.Conditions;

/// <summary>
/// A graph condition to see if a crafter has a certain component. Useful if you want certain
/// recipes only accessible by "knowledge" or some other mechanic. Only works if the item
/// is parented to their character, which, I know, drawback.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class RevolutionaryCrafterHasRecipeComponent : IGraphCondition
{
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

        if (!minds.TryGetMind(transformComponent.ParentUid, out _, out var mindComp))
            return false;

        if (mindComp.CurrentEntity == null)
            return false;

        var headRevComp = entityManager.GetComponentOrNull<HeadRevolutionaryComponent>(mindComp.CurrentEntity);

        return headRevComp != null;
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
