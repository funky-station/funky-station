using Robust.Shared.Utility;

namespace Content.Server._Impstation.Borgs.FreeformLaws;

/// <summary>
/// Adds a verb to allow custom law entry on SiliconLawProviders. Should probably never be added to anything that isn't a lawboard.
/// </summary>
[RegisterComponent]
public sealed partial class FreeformLawEntryComponent : Component
{
    [DataField]
    public LocId VerbName = "silicon-law-ui-verb";

    [DataField]
    public SpriteSpecifier? VerbIcon = null;
}
