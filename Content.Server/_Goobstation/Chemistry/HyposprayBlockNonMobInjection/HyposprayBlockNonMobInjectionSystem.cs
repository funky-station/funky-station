// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Goobstation.Chemistry.HyposprayBlockNonMobInjection;

public sealed class HyposprayBlockNonMobInjectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, AfterInteractEvent>(OnAfterInteract, before: new []{typeof(HypospraySystem)});
        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, MeleeHitEvent>(OnAttack, before: new []{typeof(HypospraySystem)});
        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, UseInHandEvent>(OnUseInHand, before: new []{typeof(HypospraySystem)});
    }

    private void OnUseInHand(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref UseInHandEvent args)
    {
        if (!IsMob(args.User))
            args.Handled = true;
    }

    private void OnAttack(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0 || !IsMob(args.HitEntities[0]))
            args.Handled = true;
    }

    private void OnAfterInteract(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !IsMob(args.Target.Value))
            args.Handled = true;
    }

    private bool IsMob(EntityUid uid)
    {
        return HasComp<MobStateComponent>(uid);
    }
}
