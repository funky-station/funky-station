using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.Revolutionary.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Construction.Conditions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CrafterHasComponent : IGraphCondition
{
    [DataField("componentName")]
    public EntProtoId ComponentName;

    [DataField("guideText")]
    public string? GuideText { get; private set; }
    [DataField("guideIcon")]
    public SpriteSpecifier? GuideIcon { get; private set; }

    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        return true;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        throw new NotImplementedException();
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

