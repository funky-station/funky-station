using Content.Server.Body.Components;
using Content.Shared.Body.Prototypes;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.EntityConditions.Conditions;

/// <summary>
///     Requires that the metabolizing organ is or is not tagged with a certain MetabolizerType
/// </summary>
[Serializable]
public sealed partial class OrganType : EntityCondition
{
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<MetabolizerTypePrototype>))]
    public string Type = default!;

    /// <summary>
    ///     Does this condition pass when the organ has the type, or when it doesn't have the type?
    /// </summary>
    [DataField]
    public bool ShouldHave = true;

    public override bool RaiseEvent(EntityUid uid, IEntityConditionRaiser raiser)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        // Check if the entity has a MetabolizerComponent
        if (!entMan.TryGetComponent<MetabolizerComponent>(uid, out var metabolizer))
            return !ShouldHave;

        // Check if the metabolizer has the required type
        bool hasType = metabolizer.MetabolizerTypes != null &&
                       metabolizer.MetabolizerTypes.Contains(Type);

        return hasType == ShouldHave;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        var metabolizerType = prototype.Index<MetabolizerTypePrototype>(Type);

        if (ShouldHave)
        {
            return Loc.GetString("reagent-effect-condition-guidebook-organ-type-requires",
                ("name", metabolizerType.LocalizedName));
        }
        else
        {
            return Loc.GetString("reagent-effect-condition-guidebook-organ-type-forbids",
                ("name", metabolizerType.LocalizedName));
        }
    }
}
