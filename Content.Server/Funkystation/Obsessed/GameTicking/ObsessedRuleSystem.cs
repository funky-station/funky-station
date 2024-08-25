using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.Changeling;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System.Text;
using Content.Shared.Obsessed;

namespace Content.Server.GameTicking.Rules;

public sealed partial class ObsessedRuleSystem : GameRuleSystem<ObsessedRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;

    public readonly ProtoId<AntagPrototype> ObsessedPrototypeId = "Obsessed";

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/Funkystation/Ambience/angels_harp_sound.ogg");

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
        var briefingShort = Loc.GetString("obsessed-role-greeting-short");

        EnsureComp<ObsessedComponent>(target);

        _antag.SendBriefing(target, briefing, Color.Pink, BriefingSound);
        _role.MindAddRole(mindId, new RoleBriefingComponent { Briefing = briefingShort }, mind, true);

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
}
