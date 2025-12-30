using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationTrichochromaticShiftComponent : Component
{
    /// <summary>
    /// The prototype ID of the action to grant.
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "ActionTrichochromaticShift";

    /// <summary>
    /// The spawned action entity, so we can remove it later.
    /// </summary>
    public EntityUid? GrantedAction;

    /// <summary>
    /// Saved original hair markings and colors when the mutation is gained.
    /// </summary>
    public List<(string MarkingId, List<Color> Colors)>? OriginalHairMarkings { get; set; }

    /// <summary>
    /// Saved original facial hair markings and colors when the mutation is gained.
    /// </summary>
    public List<(string MarkingId, List<Color> Colors)>? OriginalFacialHairMarkings { get; set; }

    /// <summary>
    /// How many times the action has been used since the last reset to original.
    /// </summary>
    public int UsesSinceOriginal { get; set; } = 0;
}
