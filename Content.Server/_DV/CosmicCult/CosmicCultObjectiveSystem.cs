// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Objectives.Components;
using Content.Shared._DV.Roles;
using Content.Shared.Ninja.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Warps;
using Robust.Shared.Random;

namespace Content.Server.Objectives.Systems;

public sealed class CosmicCultObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicEffigyConditionComponent, RequirementCheckEvent>(OnEffigyRequirementCheck);
        SubscribeLocalEvent<CosmicEffigyConditionComponent, ObjectiveAfterAssignEvent>(OnEffigyAfterAssign);

        SubscribeLocalEvent<CosmicEntropyConditionComponent, ObjectiveGetProgressEvent>(OnGetEntropyProgress);
        SubscribeLocalEvent<CosmicTierConditionComponent, ObjectiveGetProgressEvent>(OnGetTierProgress);
        SubscribeLocalEvent<CosmicVictoryConditionComponent, ObjectiveGetProgressEvent>(OnGetVictoryProgress);
    }

    private void OnEffigyRequirementCheck(EntityUid uid, CosmicEffigyConditionComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled || !_roles.MindHasRole<CosmicColossusRoleComponent>(args.MindId))
            return;

        var warps = new List<EntityUid>();
        var query = EntityQueryEnumerator<BombingTargetComponent, WarpPointComponent>();
        while (query.MoveNext(out var warpUid, out _, out var warp))
        {
            if (warp.Location != null)
            {
                warps.Add(warpUid);
            }
        }

        if (warps.Count <= 0)
        {
            args.Cancelled = true;
            return;
        }
        comp.EffigyTarget = _random.Pick(warps);
    }

    private void OnEffigyAfterAssign(EntityUid uid, CosmicEffigyConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        string description;
        if (comp.EffigyTarget == null || !TryComp<WarpPointComponent>(comp.EffigyTarget, out var warp) || warp.Location == null)
        {
            // this should never really happen but eh
            description = Loc.GetString("objective-condition-effigy-no-target");
        }
        else
        {
            description = Loc.GetString("objective-condition-effigy", ("location", warp.Location));
        }
        _metaData.SetEntityDescription(uid, description, args.Meta);
    }

    private void OnGetEntropyProgress(Entity<CosmicEntropyConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = Progress(ent.Comp.Siphoned, _number.GetTarget(ent.Owner));
    }

    private void OnGetTierProgress(Entity<CosmicTierConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = Progress(ent.Comp.Tier, _number.GetTarget(ent.Owner));
    }

    private void OnGetVictoryProgress(Entity<CosmicVictoryConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = ent.Comp.Victory ? 1f : 0f;
    }

    private static float Progress(int recruited, int target)
    {
        // prevent divide-by-zero
        if (target == 0)
            return 1f;

        return MathF.Min(recruited / (float)target, 1f);
    }
}
