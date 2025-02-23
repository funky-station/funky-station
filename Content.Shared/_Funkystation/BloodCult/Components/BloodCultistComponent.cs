using Content.Shared.Heretic.Prototypes;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult.Prototypes;

namespace Content.Shared.BloodCult;

/// <summary>
/// A Blood Cultist.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodCultistComponent : Component
{
	/// <summary>
	///		Currently active spells.
	/// </summary>
	[DataField, AutoNetworkedField] public List<ProtoId<CultAbilityPrototype>> KnownSpells = new();

	/// <summary>
    ///     Stores captured blood.
    /// </summary>
    [DataField] public int Blood = 0;

	/// <summary>
    ///     Stores if the cultist was revived in the last tick.
    /// </summary>
	[DataField] public bool BeingRevived = false;

	/// <summary>
	///		Studies the veil.
	/// </summary>
	[DataField] public bool StudyingVeil = false;

	/// <summary>
	/// The Uid of the person trying to revive the cultist.
	/// </summary>
	[DataField] public EntityUid? ReviverUid = null;

	[DataField] public SacrificingData? Sacrifice = null;
	[DataField] public ConvertingData? Convert = null;

	/// <summary>
	/// The list of sacrifice targets.
	/// </summary>
	[DataField] public List<EntityUid> Targets = new List<EntityUid>();

	public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "BloodCultFaction";
/*
    #region Prototypes

    //[DataField] public List<ProtoId<HereticKnowledgePrototype>> BaseKnowledge = new()
    //{
    //    "BreakOfDawn",
    //    "HeartbeatOfMansus",
    //    "AmberFocus",
    //    "LivingHeart",
    //    "CodexCicatrix",
    //};

	//[DataField] public List<ProtoId<BloodCultSpellPrototype>> CurrentSpells = new()
	//{
	//};

    #endregion

    [DataField, AutoNetworkedField] public List<ProtoId<HereticRitualPrototype>> KnownRituals = new();
    [DataField] public ProtoId<HereticRitualPrototype>? ChosenRitual;

    /// <summary>
    ///     Contains the list of targets that are eligible for sacrifice.
    /// </summary>
    [DataField, AutoNetworkedField] public List<NetEntity?> SacrificeTargets = new();

    /// <summary>
    ///     How much targets can a heretic have?
    /// </summary>
    [DataField, AutoNetworkedField] public int MaxTargets = 5;

    // hardcoded paths because i hate it
    // "Ash", "Lock", "Flesh", "Void", "Blade", "Rust"
    /// <summary>
    ///     Indicates a path the heretic is on.
    /// </summary>
    [DataField, AutoNetworkedField] public string? CurrentPath = null;

    /// <summary>
    ///     Indicates a stage of a path the heretic is on. 0 is no path, 10 is ascension
    /// </summary>
    [DataField, AutoNetworkedField] public int PathStage = 0;

    [DataField, AutoNetworkedField] public bool Ascended = false;

    /// <summary>
    ///     Used to prevent double casting mansus grasp.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool MansusGraspActive = false;

    /// <summary>
    ///     Indicates if a heretic is able to cast advanced spells.
    ///     Requires wearing focus, codex cicatrix, hood or anything else that allows him to do so.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool CanCastSpells = false;

    /// <summary>
    ///     dunno how to word this
    ///     its for making sure the next point update is 20 minutes in
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan NextPointUpdate;

    /// <summary>
    ///     dunno how to word this
    ///     its for making sure the next point update is 20 minutes in
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan PointCooldown = TimeSpan.FromMinutes(20);

    /// <summary>
    ///     when the time delta alert happens
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan AlertTime;

    /// <summary>
    ///     how long 2 wait
    /// </summary>
    [DataField, AutoNetworkedField] public TimeSpan AlertWaitTime = TimeSpan.FromSeconds(10);
	*/
}

public struct SacrificingData
{
	public EntityUid Target;
	public EntityUid[] Invokers;

	public SacrificingData(EntityUid target, EntityUid[] invokers)
	{
		Target = target;
		Invokers = invokers;
	}
}

public struct ConvertingData
{
	public EntityUid Target;
	public EntityUid[] Invokers;

	public ConvertingData(EntityUid target, EntityUid[] invokers)
	{
		Target = target;
		Invokers = invokers;
	}
}
