using Content.Server.GameTicking.Rules;
namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the BloodCultRuleSystem that stores info about winning/losing, player counts required
///	for stuff, and other round-wide stuff.
/// </summary>
[RegisterComponent, Access(typeof(BloodCultRuleSystem))]
public sealed partial class BloodCultRuleComponent : Component
{
	/// <summary>
	/// Charges available for the Revive Rune.
	/// </summary>
	[DataField] public int ReviveCharges = 3;

	/// <summary>
	/// Targets sacrificed successfully.
	/// </summary>
	[DataField] public List<EntityUid> TargetsDown = new List<EntityUid>();

	/// <summary>
	/// Current target.
	/// </summary>
	[DataField] public EntityUid? Target = null;

	/// <summary>
	/// Number of targets required to satisfy the sacrifice condition.
	/// </summary>
	[DataField] public int TargetsRequired = 2;

	/// <summary>
	/// Number of charges required to use a Revive Rune.
	/// </summary>
	[DataField] public int CostToRevive = 3;

	/// <summary>
	/// Number of charges gained for sacrificing someone.
	/// </summary>
	[DataField] public int ChargesForSacrifice = 1;

	/// <summary>
	/// Number of cultists required to sacrifice a dead player.
	/// </summary>
	[DataField] public int CultistsToSacrifice = 1;

	/// <summary>
	/// Number of cultists required to sacrifice a target player.
	/// </summary>
	[DataField] public int CultistsToSacrificeTarget = 3;

	/// <summary>
	/// Number of players required to convert a player.
	/// </summary>
	[DataField] public int CultistsToConvert = 2;
}
