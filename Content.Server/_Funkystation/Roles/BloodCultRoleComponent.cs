using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
/// A Blood Cultist.
/// </summary>
[RegisterComponent]
public sealed partial class BloodCultRoleComponent : BaseMindRoleComponent
{
	/// <summary>
    ///     Stores captured blood.
    /// </summary>
    [DataField] public int Blood = 0;
}