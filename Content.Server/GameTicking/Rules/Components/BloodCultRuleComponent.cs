using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mind;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the BloodCultRuleSystem that stores info about winning/losing, player counts required
///	for stuff, and other round-wide stuff.
/// </summary>
[RegisterComponent, Access(typeof(BloodCultRuleSystem))]
public sealed partial class BloodCultRuleComponent : Component
{
	/// <summary>
	///	Possible Nar'Sie summon locations.
	/// </summary>
	[DataField] public static List<string> PossibleVeilLocations = new List<string> {
		"DefaultStationBeaconCaptainsQuarters", "DefaultStationBeaconHOPOffice",
		"DefaultStationBeaconSecurity", "DefaultStationBeaconBrig",
		"DefaultStationBeaconWardensOffice", "DefaultStationBeaconHOSRoom",
		"DefaultStationBeaconArmory", "DefaultStationBeaconPermaBrig",
		"DefaultStationBeaconDetectiveRoom", "DefaultStationBeaconCourtroom",
		"DefaultStationBeaconLawOffice", "DefaultStationBeaconChemistry",
		"DefaultStationBeaconCMORoom", "DefaultStationBeaconMorgue",
		"DefaultStationBeaconRND", "DefaultStationBeaconServerRoom",
		"DefaultStationBeaconRDRoom", "DefaultStationBeaconRobotics",
		"DefaultStationBeaconArtifactLab", "DefaultStationBeaconAnomalyGenerator",
		"DefaultStationBeaconCargoReception", "DefaultStationBeaconCargoBay",
		"DefaultStationBeaconQMRoom", "DefaultStationBeaconSalvage",
		"DefaultStationBeaconCERoom", "DefaultStationBeaconAME",
		"DefaultStationBeaconTEG", "DefaultStationBeaconTechVault",
		"DefaultStationBeaconKitchen", "DefaultStationBeaconBar",
		"DefaultStationBeaconBotany", "DefaultStationBeaconAICore",
		"DefaultStationBeaconEVAStorage", "DefaultStationBeaconChapel",
		"DefaultStationBeaconLibrary", "DefaultStationBeaconTheater",
		"DefaultStationBeaconToolRoom"
	};

	[DataField] public WeakVeilLocation? WeakVeil1 = null;
	[DataField] public WeakVeilLocation? WeakVeil2 = null;
	[DataField] public WeakVeilLocation? WeakVeil3 = null;

	/// <summary>
	///		Stores the location the existing cultists have decided to summon Nar'Sie.
	/// </summary>
	[DataField] public WeakVeilLocation? LocationForSummon = null;

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
	///	Conversions needed until glowing eyes -- set when cult is initialized.
	/// </summary>
	[DataField] public int ConversionsUntilEyes = 0;

	/// <summary>
	///	Conversions needed until rise -- set when cult is initialized.
	/// </summary>
	[DataField] public int ConversionsUntilRise = 0;

	/// <summary>
	///	Has the cult gained glowing eyes yet?
	/// </summary>
	[DataField] public bool HasEyes = false;

	/// <summary>
	///	Has the cult risen yet?
	/// </summary>
	[DataField] public bool HasRisen = false;

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
	[DataField] public EntityUid? Target = null;

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
