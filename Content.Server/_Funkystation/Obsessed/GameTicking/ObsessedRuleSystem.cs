﻿using System.Text;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared.Changeling;
using Content.Shared.Obsessed;
using Content.Shared.Roles;
using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Obsessed.GameTicking;

public sealed class ObsessedRuleSystem : GameRuleSystem<ObsessedRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;

    private readonly ObsessedRuleComponent _rules = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Funkystation/Ambience/obsessed.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObsessedRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<ObsessedRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnSelectAntag(EntityUid uid, ObsessedRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeObsessed(args.EntityUid, comp);
    }
    public bool MakeObsessed(EntityUid target, ObsessedRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        var briefing = Loc.GetString("obsessed-role-greeting");

        EnsureComp<ObsessedComponent>(target, out var obsessedComponent);

        obsessedComponent.HugAmount = 0f;

        _antag.SendBriefing(target, briefing, Color.Pink, BriefingSound);
        _role.MindAddRole(mindId, "MindRoleObsessed", mind, true);

        foreach (var objective in rule.Objectives)
        {
            _mind.TryAddObjective(mindId, mind, objective);
        }

        return true;
    }

    private void OnTextPrepend(EntityUid uid, ObsessedRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        var mostAbsorbedName = string.Empty;
        var mostStolenName = string.Empty;
        var mostAbsorbed = 0f;
        var mostStolen = 0f;

        foreach (var ling in EntityQuery<ChangelingComponent>())
        {
            if (!_mind.TryGetMind(ling.Owner, out var mindId, out var mind))
                continue;

            if (!TryComp<MetaDataComponent>(ling.Owner, out var metaData))
                continue;

            if (ling.TotalAbsorbedEntities > mostAbsorbed)
            {
                mostAbsorbed = ling.TotalAbsorbedEntities;
                mostAbsorbedName = _objective.GetTitle((mindId, mind), metaData.EntityName);
            }
            if (ling.TotalStolenDNA > mostStolen)
            {
                mostStolen = ling.TotalStolenDNA;
                mostStolenName = _objective.GetTitle((mindId, mind), metaData.EntityName);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString($"roundend-prepend-changeling-absorbed{(!string.IsNullOrWhiteSpace(mostAbsorbedName) ? "-named" : "")}", ("name", mostAbsorbedName), ("number", mostAbsorbed)));
        sb.AppendLine(Loc.GetString($"roundend-prepend-changeling-stolen{(!string.IsNullOrWhiteSpace(mostStolenName) ? "-named" : "")}", ("name", mostStolenName), ("number", mostStolen)));

        args.Text = sb.ToString();
    }

    public void ObsessedCompletedObjectives(EntityUid mind)
    {
        if (!_mind.TryGetMind(mind, out var mindId, out var mindComponent))
            return;

        for (var i = 0; i < _rules.Objectives.Count; i++)
        {
            _mind.TryRemoveObjective(mindId, mindComponent, i);
        }

        EnsureComp<ObsessedComponent>((EntityUid) mindComponent.OwnedEntity!, out var obsessedComponent);

        _mind.TryAddObjective(mindId, mindComponent, _rules.Murder);
        _antag.SendBriefing(mindComponent.Session, Loc.GetString("obsessed-role-murder", ("target", obsessedComponent.TargetName)), Color.Pink, BriefingSound);
    }
}
