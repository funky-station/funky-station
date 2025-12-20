// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Electrocution;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Electrocution;
using Robust.Shared.Random;
using Content.Server._EinsteinEngines.Power.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._EinsteinEngines.Power.Systems;

public sealed class BatteryElectrocuteChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, ElectrocutedEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(EntityUid uid, BatteryComponent battery, ElectrocutedEvent args)
    {
        if (args.ShockDamage is null || args.ShockDamage <= 0)
            return;

        var addedCharge =
            Math.Min(
                args.ShockDamage.Value * args.SiemensCoefficient
                / ElectrocutionSystem.ElectrifiedDamagePerWatt * 2f,
                battery.MaxCharge * 0.25f)
            * _random.NextFloat(0.75f, 1.25f);

        var currentCharge = _battery.GetCharge(uid);
        var newCharge = Math.Min(currentCharge + addedCharge, battery.MaxCharge);

        _battery.SetCharge(uid, newCharge);

        _popup.PopupEntity(Loc.GetString("battery-electrocute-charge"), uid, uid);
    }
}
