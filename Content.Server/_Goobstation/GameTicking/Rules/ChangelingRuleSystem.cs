// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 ALooseGoose <ALooseGoosey@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

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
using Robust.Shared.Random;
using System.Text;
using Content.Shared._EinsteinEngines.Silicon.Components;

namespace Content.Server.GameTicking.Rules;

public sealed partial class ChangelingRuleSystem : GameRuleSystem<ChangelingRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/changeling_start.ogg");

    public readonly ProtoId<AntagPrototype> ChangelingPrototypeId = "Changeling";

    public readonly ProtoId<NpcFactionPrototype> ChangelingFactionId = "Changeling";

    public readonly ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

    public readonly ProtoId<CurrencyPrototype> Currency = "EvolutionPoint";

    public static readonly EntProtoId MindRole = "MindRoleChangeling";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<ChangelingRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnSelectAntag(EntityUid uid, ChangelingRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeChangeling(args.EntityUid, comp);
    }
    public bool MakeChangeling(EntityUid target, ChangelingRuleComponent rule)
    {
        if (HasComp<SiliconComponent>(target))
            return false;

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, MindRole, mind, true);


        // briefing
        rule.ChangelingHive = "ChangelingFlavor";
        var hive = _random.Pick(_prototypeManager.Index(rule.ChangelingHive).Values);
        rule.ChangelingHive = hive;

        if (TryComp<MetaDataComponent>(target, out var metaData))
        {
            var briefing = new StringBuilder();
            briefing.AppendLine(Loc.GetString("changeling-role-greeting-intro", ("name", metaData?.EntityName ?? "Unknown")));
            briefing.AppendLine(Loc.GetString($"changeling-role-greeting-{rule.ChangelingHive}"));
            briefing.AppendLine(Loc.GetString($"changeling-role-greeting-objectives"));
            briefing.AppendLine(Loc.GetString($"changeling-role-greeting-final"));

            var briefingshort = new StringBuilder();
            briefingshort.AppendLine("\n" + Loc.GetString("changeling-role-greeting-short", ("name", metaData?.EntityName ?? "Unknown")));
            briefingshort.AppendLine("\n" + Loc.GetString($"changeling-{rule.ChangelingHive}-intro"));
            briefingshort.AppendLine("\n" + Loc.GetString($"changeling-{rule.ChangelingHive}-doctrine"));
            briefingshort.AppendLine("\n" + Loc.GetString($"changeling-{rule.ChangelingHive}-allies"));

            _antag.SendBriefing(target, briefing.ToString(), Color.Yellow, BriefingSound);

            if (_role.MindHasRole<ChangelingRoleComponent>(mindId, out var mr))
                AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefingshort.ToString() }, overwrite: true);
        }
        // hivemind stuff
        _npcFaction.RemoveFaction(target, NanotrasenFactionId, false);
        _npcFaction.AddFaction(target, ChangelingFactionId);

        // make sure it's initial chems are set to max
        EnsureComp<ChangelingComponent>(target);

        // add store
        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(Currency);
        store.Balance.Add(Currency, 16);

        rule.ChangelingMinds.Add(mindId);

        return true;
    }

    private void OnTextPrepend(EntityUid uid, ChangelingRuleComponent comp, ref ObjectivesTextPrependEvent args)
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
