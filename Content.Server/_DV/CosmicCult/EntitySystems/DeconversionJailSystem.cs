// SPDX-FileCopyrightText: 2026 AftrLite <61218133+AftrLite@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Actions;
using Content.Server.BloodCult.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Heretic.Abilities;
using Content.Server.Heretic.Components;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.BloodCult;
using Content.Shared.Heretic; // HERETIC CODE
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Store.Components;

namespace Content.Server._DV.CosmicCult;
public sealed partial class DeconversionJailSystem : SharedDeconversionJailSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly BloodCultMindShieldSystem _bloodCult = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, OubliettePurgeAttemptEvent>(PurgeBloodCult);
        SubscribeLocalEvent<HereticComponent, OubliettePurgeAttemptEvent>(PurgeHeretic); // HERETIC CODE
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DeconversionOublietteComponent>();

        while (query.MoveNext(out _, out var comp))
        {
            if (comp.OublietteState == OublietteStates.Active && Timing.CurTime > comp.EmoteTime && comp.Victim is not null)
            {
                comp.EmoteTime = Timing.CurTime + Random.Next(comp.EmoteMinTime, comp.EmoteMaxTime);
                _chat.TryEmoteWithChat(comp.Victim.Value, "Scream", ChatTransmitRange.Normal, false, null, true, true);
                PopUp.PopupEntity(Loc.GetString("cosmic-oubliette-random-horror", ("COUNT", Random.Next(1, 7))), comp.Victim.Value, comp.Victim.Value, PopupType.MediumCaution);
            }
        }
    }

    private void PurgeBloodCult(Entity<BloodCultistComponent> ent, ref OubliettePurgeAttemptEvent args)
    {
        _bloodCult.TryDeconvert(args.Target);
        OublietteSuccess(args.Oubliette, args.Target);

        args.Handled = true;
    }

    // BEGIN HERETIC CODE - Wow, it's dogshit. This surprises nobody.
    private void PurgeHeretic(Entity<HereticComponent> ent, ref OubliettePurgeAttemptEvent args)
    {
        if (!_mind.TryGetMind(ent, out var mindId, out _) || !TryComp<MindComponent>(mindId, out var mindComp))
            return;

        _role.MindTryRemoveRole<HereticRoleComponent>(mindId);
        _role.MindTryRemoveRole<RoleBriefingComponent>(mindId);

        RemComp<HereticComponent>(ent);
        RemComp<StoreComponent>(ent);
        if (HasComp<AristocratComponent>(ent))
            RemComp<AristocratComponent>(ent);
        if (HasComp<HereticFlamesComponent>(ent))
            RemComp<HereticFlamesComponent>(ent);

        foreach (var actionEnt in ent.Comp.ActionEntities)
            _actions.RemoveAction(actionEnt);

        EnsureComp<HereticComponent>(ent);
        _role.MindAddRole(mindId, "MindRoleHeretic", mindComp, true);
        _role.MindHasRole<HereticRoleComponent>(mindId, out var hereticRole);

        var briefingShort = Loc.GetString("heretic-role-greeting-short");
        if (hereticRole is not null)
            AddComp(hereticRole.Value, new RoleBriefingComponent { Briefing = briefingShort }, overwrite: true);

        if (_mind.TryFindObjective((mindId, mindComp), "HereticKnowledgeObjective", out var knowObj)
            && TryComp<HereticKnowledgeConditionComponent>(knowObj, out var knowObjComp))
            knowObjComp.Researched = 0;

        if (_mind.TryFindObjective((mindId, mindComp), "HereticSacrificeObjective", out var crewObj)
            && TryComp<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp))
            crewObjComp.Sacrificed = 0;

        if (_mind.TryFindObjective((mindId, mindComp), "HereticSacrificeHeadObjective", out var crewHeadObj)
            && TryComp<HereticSacrificeConditionComponent>(crewHeadObj, out var crewHeadObjComp)
            && crewHeadObjComp.IsCommand)
            crewHeadObjComp.Sacrificed = 0;

        var store = EnsureComp<StoreComponent>(ent);
        store.Categories.Add("HereticPathAsh");
        store.Categories.Add("HereticPathFlesh");
        store.Categories.Add("HereticPathVoid");
        store.Categories.Add("HereticPathSide");
        store.CurrencyWhitelist.Add("KnowledgePoint");
        store.Balance.Add("KnowledgePoint", 2);

        PopUp.PopupEntity("A TERRIBLE EMPTINESS FALLS UPON YOUR MIND.", ent, ent, PopupType.LargeCaution); // I'm not dignifying heretic shitcode with localization entries. Fuck you.
        OublietteSuccess(args.Oubliette, args.Target);
        args.Handled = true;
    }
    // END HERETIC CODE
}
