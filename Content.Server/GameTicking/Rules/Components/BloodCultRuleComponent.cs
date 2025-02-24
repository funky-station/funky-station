using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mind;
using Content.Shared.BloodCult;

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
	/// Total sacrifices made.
	/// </summary>
	[DataField] public int TotalSacrifices = 0;

	/// <summary>
	/// Targets sacrificed successfully.
	/// </summary>
	[DataField] public List<EntityUid> TargetsDown = new List<EntityUid>();

	/// <summary>
	/// Nar'Sie ready to summon.
	/// </summary>
	[DataField] public bool VeilWeakened = false;

	/// <summary>
	/// Whether or not the VeilWeakened announcement has played.
	/// </summary>
	[DataField] public bool VeilWeakenedAnnouncementPlayed = false;

	/// <summary>
	///	Have the cultists won?
	/// </summary>
	[DataField] public bool CultistsWin = false;

	[DataField] public TimeSpan? CultVictoryEndTime = null;
	[DataField] public bool CultVictoryAnnouncementPlayed = false;

	/// <summary>
	///	Time in seconds after Nar'Sie spawns for the shuttle to be called.
	/// </summary>
	[DataField] public TimeSpan CultVictoryEndDelay = TimeSpan.FromSeconds(15);

	/// <summary>
	/// Time after the evac shuttle is dispatched for it to arrive.
	/// </summary>
	[DataField] public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(2);

	/// <summary>
	/// Current target.
	/// </summary>
	[DataField] public Entity<MindComponent>? Target = null;

	// <summary>
	/// When to give initial report on cultist count and crew count.
	/// </summary>
	[DataField] public TimeSpan? InitialReportTime = null;

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
