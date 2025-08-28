// SPDX-FileCopyrightText: 2025 UristMcWiki <endernate2015@gmail.com>
// SPDX-FileCopyrightText: 2025 amatwiedle <amatwiedle@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Store;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Allows a store entry to be filtered out based on whether war has been declared or not. Almost entirely made by Amatwiedle on Funky Station Discord. Collaboration with Urist McWiki.
/// </summary>
public sealed partial class WarOpsCondition : ListingCondition
{
    public override bool Condition(ListingConditionArgs args)
    {
        var esm = IoCManager.Resolve<IEntitySystemManager>();

        var ent = args.EntityManager;
        var gameTicker = esm.GetEntitySystem<GameTicker>();

        var rules = gameTicker.GetActiveGameRules();
        foreach (var rule in rules)
        {
            if (ent.TryGetComponent<NukeopsRuleComponent>(rule, out var nukeops)) //fetches in moment ruling on war ops
            {
                if (nukeops.WarDeclaredTime != null) return true;
                else return false;

            }
        }

        return false;
    }
}
