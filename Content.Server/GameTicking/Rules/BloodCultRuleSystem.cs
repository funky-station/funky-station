using Content.Shared.GameTicking.Components;
using Content.Server.Roles;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Where all the main stuff for Blood Cults happen
/// </summary>
public sealed class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
	[Dependency] private readonly AntagSelectionSystem _antag = default!;
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly RoleSystem _role = default!;

	public readonly string CultComponentId = "BloodCultist";

	[ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleCultist";

	public override void Initialize()
	{
		base.Initialize();
		//SubscribeLocalEvent<CommandStaffComponent, MobStateChangedEvent>(OnCommandMobStateChanged);

		// Do we need a special "head" cultist? Don't think so
        //SubscribeLocalEvent<HeadRevolutionaryComponent, MobStateChangedEvent>(OnHeadRevMobStateChanged);

		SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected); // Funky Station
        SubscribeLocalEvent<BloodCultRoleComponent, GetBriefingEvent>(OnGetBriefing);

		// Do we need a special "head" cultist? Don't think so
		//SubscribeLocalEvent<HeadRevolutionaryComponent, AfterFlashedEvent>(OnPostFlash);
	}

	protected override void Started(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        //component.CommandCheck = _timing.CurTime + component.TimerWait;
    }

	private void AfterEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
		Console.WriteLine("SELECTED CULTIST ENTITY!!!");
        MakeCultist(args.EntityUid, ent);
    }

	/// <summary>
    /// Supplies new cultists with what they need.
    /// </summary>
    /// <returns>true if uplink was successfully added.</returns>
    private bool MakeCultist(EntityUid traitor, BloodCultRuleComponent component)
    {
        if (!_mind.TryGetMind(traitor, out var mindId, out var mind))
            return false;

		_role.MindAddRole(mindId, mindRole.Id, mind, true);
		//_role.MindAddRole(mindId, CultComponentId, mind, true);
		//_role.AddRole(mindId, "BloodCultRole", mind, true);

        //var pda = _uplink.FindUplinkTarget(traitor);
        //if (pda == null || !_uplink.AddUplink(traitor, component.StartingBalance, component.UplinkCurrencyId, component.UplinkStoreId))
        //    return false;

        //var code = EnsureComp<RingerUplinkComponent>(pda.Value).Code;

		EnsureComp<BloodCultistComponent>(traitor);

        _antag.SendBriefing(traitor, Loc.GetString("cult-role-greeting"), Color.Red, null);

        if (_role.MindHasRole<BloodCultRoleComponent>(mindId, out var cultRoleComp))
			AddComp(cultRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("cult-briefing") }, overwrite: true);
			//AddComp(cultRoleComp.Value, new RoleBriefingComponent { Briefing = "GET READY TO CULT" }, overwrite: true);
            //AddComp(cultRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("head-rev-briefing", ("code", string.Join("-", code).Replace("sharp", "#"))) }, overwrite: true);

        return true;
    }

	protected override void ActiveTick(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

		// Step 1: Check to see if the Cultists have lost.
        //if (component.RevLossTimerActive && !component.RevForceLose)
        //{
        //    var headRevList = GetHeadRevs();

        //    if (!IsGroupDetainedOrDead(headRevList, true, false))
        //    {
        //        component.RevLossTimerActive = false;

        //        for (int i = 0; i < headRevList.Count; i++)
        //        {
        //            _popup.PopupEntity(Loc.GetString("rev-headrev-returned"), headRevList[i], headRevList[i]);
        //        }
        //    }
        //    else if (component.RevLoseTime <= _timing.CurTime)
        //    {
        //        component.RevForceLose = true;
        //        for (int i = 0; i < headRevList.Count; i++)
        //        {
        //            _popup.PopupEntity(Loc.GetString("rev-headrev-abandoned"), headRevList[i], headRevList[i]);
        //        }
        //    }
        //}

		// Step 2: Check to see if the Cultists have timed out. (?)
        // funkystation
        //if (component.RevVictoryEndTime != null && _timing.CurTime >= component.RevVictoryEndTime)
        //{
        //    EndRound();

        //    return;
        //}

		// Step 3: Check to see if the Cultists have summoned Nar'Sie.
        //if (component.CommandCheck <= _timing.CurTime)
        //{
        //    component.CommandCheck = _timing.CurTime + component.TimerWait;

        //    // goob edit
        //    if (CheckCommandLose())
        //    {
        //        if (!component.HasRevAnnouncementPlayed)
        //        {
        //            _chatSystem.DispatchGlobalAnnouncement(
        //                Loc.GetString("revolutionaries-win-announcement"),
        //                Loc.GetString("revolutionaries-win-sender"),
        //                colorOverride: Color.Gold);

        //            component.HasRevAnnouncementPlayed = true;

        //            component.RevVictoryEndTime = _timing.CurTime + component.RevVictoryEndDelay;
        //        }
        //    }

        //    if (CheckRevsLose() && !component.HasAnnouncementPlayed)
        //    {
        //        DeconvertAllRevs();

        //        _roundEnd.DoRoundEndBehavior(RoundEndBehavior.ShuttleCall,
        //            component.ShuttleCallTime,
        //            textCall: "revolutionaries-lose-announcement-shuttle-call",
        //            textAnnounce: "revolutionaries-lose-announcement");

        //        component.HasAnnouncementPlayed = true;
        //    }
        //}
    }

	protected override void AppendRoundEndText(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

		// Which team won?
        //var revsLost = CheckRevsLose();
        //var commandLost = CheckCommandLose();
        
		// This is (revsLost, commandsLost) concatted together
        // (moony wrote this comment idk what it means)
        //var index = (commandLost ? 1 : 0) | (revsLost ? 2 : 0);
        args.AddLine("The super duper cult round is over!");//Loc.GetString(Outcomes[index]));

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine("This is where the number of cultists will go!");//Loc.GetString("rev-headrev-count", ("initialCount", sessionData.Count)));

		//foreach (var (mind, data, name) in sessionData)
        //{
        //    _role.MindHasRole<BloodCultRoleComponent>(mind, out var role);
        //    var count = CompOrNull<BloodCultRoleComponent>(role)?.ConvertedCount ?? 0;

        //    args.AddLine(Loc.GetString("rev-headrev-name-user",
        //        ("name", name),
        //        ("username", data.UserName),
        //        ("count", count)));

            // TODO: someone suggested listing all alive? revs maybe implement at some point
        //}
    }

	private void OnGetBriefing(EntityUid uid, BloodCultRoleComponent comp, ref GetBriefingEvent args)
    {
		args.Append(Loc.GetString("cult-briefing-targets"));
    }
}
