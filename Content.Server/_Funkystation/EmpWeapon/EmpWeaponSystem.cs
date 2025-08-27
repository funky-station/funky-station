// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Funkystation.EmpWeapon.Components;
using Content.Server.Emp;
using Content.Shared._Funkystation.EmpWeapon;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using System.Linq;

namespace Content.Server._Funkystation.EmpWeapon
{
    public sealed class EmpWeaponSystem : SharedEmpWeaponSystem
    {
        [Dependency] private readonly EmpSystem _empSystem = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly SharedChargesSystem _charges = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EmpWeaponComponent, MeleeHitEvent>(OnEmpWeaponHit);
        }

        private void OnEmpWeaponHit(EntityUid uid, EmpWeaponComponent comp, MeleeHitEvent args)
        {
            if (!args.IsHit || !args.HitEntities.Any())
                return;

            args.Handled = true;
            foreach (var e in args.HitEntities)
            {
                if (comp.RequiresCharges)
                {
                    if (!TryComp<LimitedChargesComponent>(uid, out var charge) ||
                        _charges.IsEmpty(uid, charge))
                        return;
                    _charges.UseCharge(uid, charge);
                }
                var xform = Transform(args.HitEntities.FirstOrDefault());
                _empSystem.EmpPulse(_transform.GetMapCoordinates(xform), comp.EmpRange, comp.EmpConsumption, comp.EmpDuration);
            }
        }
    }

}
