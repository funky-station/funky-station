// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class ChangelingObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbConditionComponent, ObjectiveGetProgressEvent>(OnAbsorbGetProgress);
        SubscribeLocalEvent<StealDNAConditionComponent, ObjectiveGetProgressEvent>(OnStealDNAGetProgress);
    }

    private void OnAbsorbGetProgress(EntityUid uid, AbsorbConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target != 0)
            args.Progress = MathF.Min(comp.Absorbed / target, 1f);
        else args.Progress = 1f;
    }
    private void OnStealDNAGetProgress(EntityUid uid, StealDNAConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target != 0)
            args.Progress = MathF.Min(comp.DNAStolen / target, 1f);
        else args.Progress = 1f;
    }
}
