// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class CosmicCultObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicEntropyConditionComponent, ObjectiveGetProgressEvent>(OnGetEntropyProgress);
        SubscribeLocalEvent<CosmicTierConditionComponent, ObjectiveGetProgressEvent>(OnGetTierProgress);
        SubscribeLocalEvent<CosmicVictoryConditionComponent, ObjectiveGetProgressEvent>(OnGetVictoryProgress);
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
