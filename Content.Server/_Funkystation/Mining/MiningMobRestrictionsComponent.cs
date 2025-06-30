namespace Content.Server._Funkystation.Mining;

// This is dumb
/// <summary>
/// Deletes the attached entity if the linked entity is deleted.
/// </summary>
[RegisterComponent]
public sealed partial class MiningMobRestrictionsComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid LinkedEntity;
}
