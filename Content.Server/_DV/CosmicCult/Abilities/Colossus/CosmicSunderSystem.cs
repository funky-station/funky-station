// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Stunnable; // Funky
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSunderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!; // Funky

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusSunder>(OnColossusSunder);
    }

    private void OnColossusSunder(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusSunder args)
    {
        args.Handled = true;

        var comp = ent.Comp;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Action);
        _transform.SetCoordinates(ent, args.Target);
        _transform.AnchorEntity(ent);
        _stun.TryStun(ent, comp.AttackWait + comp.TeleportWait, true); // Funky

        //Begin Funky changes
        comp.Teleporting = true;
        comp.AttackHoldTimer = comp.AttackWait + _timing.CurTime + comp.TeleportWait;
        comp.TeleportTimer = _timing.CurTime + comp.TeleportWait;
        //TODO spawn some cool telegraph effects
        //Spawn(comp.Attack1Vfx, args.Target);

        //var detonator = Spawn(comp.TileDetonations, args.Target);
        //EnsureComp<CosmicTileDetonatorComponent>(detonator, out var detonateComp);
        //detonateComp.DetonationTimer = _timing.CurTime;
        //End Funky changes
    }
}
