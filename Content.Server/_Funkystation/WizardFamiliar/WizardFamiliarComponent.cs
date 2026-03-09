namespace Content.Server._Funkystation.WizardFamiliar;

/// <summary>
/// Marks an entity as a wizard's familiar. Stores the wizard who summoned them.
/// </summary>
[RegisterComponent]
public sealed partial class WizardFamiliarComponent : Component
{
    /// <summary>
    /// The wizard who summoned this familiar.
    /// </summary>
    [DataField]
    public EntityUid? Wizard;
}
