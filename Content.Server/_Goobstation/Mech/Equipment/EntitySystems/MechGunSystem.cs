// SPDX-FileCopyrightText: 2024 NULL882 <gost6865@yandex.ru>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mech.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Goobstation.Server.Mech.Equipment.EntitySystems;

public sealed class MechGunSystem : EntitySystem
{
    [Dependency] private readonly MechSystem _mech = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechEquipmentComponent, HandleMechEquipmentBatteryEvent>(OnHandleMechEquipmentBattery);
        SubscribeLocalEvent<BatteryAmmoProviderComponent, CheckMechWeaponBatteryEvent>(OnCheckBattery);
    }

    private void OnHandleMechEquipmentBattery(
        EntityUid uid,
        MechEquipmentComponent component,
        HandleMechEquipmentBatteryEvent args)
    {
        if (!component.EquipmentOwner.HasValue)
            return;

        if (!TryComp<MechComponent>(component.EquipmentOwner.Value, out var mech))
            return;

        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        var ev = new CheckMechWeaponBatteryEvent(uid);
        RaiseLocalEvent(uid, ref ev);

        if (ev.Cancelled)
            return;

        ChargeGunBattery(uid, battery, mech);
    }

    private void OnCheckBattery(
        EntityUid uid,
        BatteryAmmoProviderComponent component,
        ref CheckMechWeaponBatteryEvent args)
    {
        var charge = _battery.GetCharge(uid);

        if (charge >= component.FireCost)
            args.Cancelled = true;
    }

    private void ChargeGunBattery(
        EntityUid uid,
        BatteryComponent battery,
        MechComponent mech)
    {
        if (!TryComp<MechEquipmentComponent>(uid, out var mechEquipment))
            return;

        if (!mechEquipment.EquipmentOwner.HasValue)
            return;

        var currentCharge = _battery.GetCharge(uid);
        var maxCharge = battery.MaxCharge;

        var chargeDelta = maxCharge - currentCharge;

        if (chargeDelta <= 0f)
            return;

        if (mech.Energy < chargeDelta)
            return;

        if (!_mech.TryChangeEnergy(mechEquipment.EquipmentOwner.Value, -chargeDelta, mech))
            return;

        _battery.SetCharge(uid, maxCharge);
    }
}

[ByRefEvent]
public record struct CheckMechWeaponBatteryEvent(EntityUid BatteryUid, bool Cancelled = false);
