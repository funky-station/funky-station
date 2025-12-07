// SPDX-FileCopyrightText: 2025 Drywink <43855731+Drywink@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Heretic.Prototypes;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;

namespace Content.Server.Heretic.Ritual;

/// <summary>
///     Verify is the heretic has completed its objectives.
///     Return true if so.
/// </summary>
[Virtual]
public partial class RitualAscensionBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        var mindSystem = args.EntityManager.System<SharedMindSystem>();
        // get objectives
        if (mindSystem.TryGetMind(args.Performer, out var mindId, out var mind))
        {
            if (mindSystem.TryFindObjective((mindId, mind), "HereticSacrificeObjective", out var crewObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp)
                && args.EntityManager.TryGetComponent<NumberObjectiveComponent>(crewObj, out var numberObj))
            {
                var requiredSacrifices = numberObj.Target;
                if (crewObjComp.Sacrificed < requiredSacrifices)
                {
                    outstr = Loc.GetString("heretic-ritual-fail-ascension");
                    return false;
                }
            }

            if (mindSystem.TryFindObjective((mindId, mind), "HereticSacrificeHeadObjective", out var crewHeadObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewHeadObj,
                    out var crewHeadObjComp)
                && args.EntityManager.TryGetComponent<NumberObjectiveComponent>(crewHeadObj, out var numberHeadObj))
            {
                var requiredSacrifices = numberHeadObj.Target;
                if (crewHeadObjComp.Sacrificed < requiredSacrifices)
                {
                    outstr = Loc.GetString("heretic-ritual-fail-ascension");
                    return false;
                }
            }

            outstr = null;
            return true;
        }

        outstr = null;
        return false;
    }
    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}
