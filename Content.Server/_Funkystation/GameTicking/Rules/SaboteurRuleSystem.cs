using Content.Server._Funkystation.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared.Humanoid;

namespace Content.Server._Funkystation.GameTicking.Rules;

public sealed class SaboteurRuleSystem : GameRuleSystem<SaboteurRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SaboteurRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<SaboteurRuleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    // Greeting upon saboteur activation
    private void AfterAntagSelected(Entity<SaboteurRuleComponent> mindId, ref AfterAntagEntitySelectedEvent args)
    {
        var ent = args.EntityUid;
        _antag.SendBriefing(ent, MakeBriefing(ent), null, null);
    }

    // Character screen briefing
    private void OnGetBriefing(Entity<SaboteurRuleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;
        args.Append(MakeBriefing(ent.Value));
    }

    private string MakeBriefing(EntityUid ent)
    {
        var isHuman = HasComp<HumanoidAppearanceComponent>(ent);
        var briefing = Loc.GetString("saboteur-role-greeting");

        if (isHuman)
            briefing += "\n \n" + Loc.GetString("saboteur-role-greeting-equipment") + "\n";

        return briefing;
    }
}
