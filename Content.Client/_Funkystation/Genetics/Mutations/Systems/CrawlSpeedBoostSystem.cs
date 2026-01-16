// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Abilities;
using Content.Shared._Funkystation.Genetics.Mutations.Components;
using Content.Shared._Funkystation.Genetics.Mutations.Systems;
using Content.Shared._White.Standing;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;

namespace Content.Client._Funkystation.Genetics.Systems;

public sealed class CrawlSpeedBoostSystem : SharedCrawlSpeedBoostSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlSpeedBoostComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid, CrawlSpeedBoostComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<LayingDownComponent>(uid, out var laying) ||
            !TryComp<StandingStateComponent>(uid, out var standing) ||
            standing.CurrentState != StandingState.Lying)
            return;

        float original = laying.SpeedModify;
        float boost = comp.TargetSpeedMult / original;

        args.ModifySpeed(boost, boost);
    }
}
