// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared._EinsteinEngines.Silicon.Systems;
using Content.Shared.Movement.Systems;
using Content.Server.Body.Components;
using Content.Shared.Mind.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._Goobstation.CVars;
using Content.Server.Power.EntitySystems;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using Content.Shared.PowerCell.Components;
using Content.Shared.Alert;
using Content.Shared.Atmos.Components;
using Content.Shared.CCVar;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Temperature.Components;

namespace Content.Server._EinsteinEngines.Silicon.Charge;

public sealed class SiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _moveMod = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SiliconComponent, ComponentStartup>(OnSiliconStartup);
    }
    public bool TryGetSiliconBattery(
        EntityUid silicon,
        [NotNullWhen(true)] out BatteryComponent? batteryComp,
        [NotNullWhen(true)] out EntityUid? batteryEnt)
    {
        batteryComp = null;
        batteryEnt = null;

        if (!HasComp<SiliconComponent>(silicon))
            return false;

        if (TryComp(silicon, out batteryComp))
        {
            batteryEnt = silicon;
            return true;
        }

        if (_powerCell.TryGetBatteryFromSlot(silicon, out var battery))
        {
            batteryEnt = battery.Value.Owner;
            return TryComp(batteryEnt.Value, out batteryComp);
        }

        return false;
    }

    private void OnSiliconStartup(EntityUid uid, SiliconComponent component, ComponentStartup args)
    {
        if (!HasComp<PowerCellSlotComponent>(uid))
            return;

        if (component.EntityType.GetType() != typeof(SiliconType))
            DebugTools.Assert("SiliconComponent.EntityType is not a SiliconType enum.");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SiliconComponent>();
        while (query.MoveNext(out var silicon, out var siliconComp))
        {
            if (_mobState.IsDead(silicon) || !siliconComp.BatteryPowered)
                continue;

            if (siliconComp.EntityType.Equals(SiliconType.Npc))
            {
                var updateTime = _config.GetCVar(CCVars.SiliconNpcUpdateTime);
                if (_timing.CurTime - siliconComp.LastDrainTime < TimeSpan.FromSeconds(updateTime))
                    continue;

                siliconComp.LastDrainTime = _timing.CurTime;
            }

            if (!TryGetSiliconBattery(silicon, out var batteryComp, out var batteryEnt))
            {
                UpdateChargeState(silicon, 0, siliconComp);
                _alerts.ClearAlert(silicon, siliconComp.BatteryAlert);
                _alerts.ShowAlert(silicon, siliconComp.NoBatteryAlert);
                continue;
            }

            if (TryComp<MindContainerComponent>(silicon, out var mind) && !mind.HasMind)
                continue;

            var drainRate = siliconComp.DrainPerSecond;
            var drainRateFinalAddi = 0f;

            if (!siliconComp.EntityType.Equals(SiliconType.Npc))
                drainRateFinalAddi += SiliconHeatEffects(silicon, siliconComp, frameTime) - 1f;

            drainRate += Math.Clamp(
                drainRateFinalAddi,
                drainRate * -0.9f,
                batteryComp.MaxCharge / 240f);

            _battery.TryUseCharge(batteryEnt.Value, frameTime * drainRate);

            var charge = _battery.GetCharge(batteryEnt.Value);
            var chargePercent = (short) MathF.Round(charge / batteryComp.MaxCharge * 10f);

            UpdateChargeState(silicon, chargePercent, siliconComp);
        }
    }

    public void UpdateChargeState(EntityUid uid, short chargePercent, SiliconComponent component)
    {
        component.ChargeState = chargePercent;

        RaiseLocalEvent(uid, new SiliconChargeStateUpdateEvent(chargePercent));
        _moveMod.RefreshMovementSpeedModifiers(uid);

        if (_alerts.IsShowingAlert(uid, component.NoBatteryAlert) && chargePercent > 0)
        {
            _alerts.ClearAlert(uid, component.NoBatteryAlert);
            _alerts.ShowAlert(uid, component.BatteryAlert, chargePercent);
        }
    }

    private float SiliconHeatEffects(EntityUid silicon, SiliconComponent siliconComp, float frameTime)
    {
        if (!TryComp<TemperatureComponent>(silicon, out var temp)
            || !TryComp<ThermalRegulatorComponent>(silicon, out var thermal))
            return 0;

        var upper = thermal.NormalBodyTemperature + thermal.ThermalRegulationTemperatureThreshold;
        var upperHalf = thermal.NormalBodyTemperature + thermal.ThermalRegulationTemperatureThreshold * 0.5f;

        if (temp.CurrentTemperature > upperHalf)
        {
            var hotMulti = Math.Min(temp.CurrentTemperature / upperHalf, 4f);

            siliconComp.OverheatAccumulator += frameTime;
            if (siliconComp.OverheatAccumulator < 5f)
                return hotMulti;

            siliconComp.OverheatAccumulator -= 5f;

            if (!TryComp<FlammableComponent>(silicon, out var flam)
                || flam.OnFire
                || temp.CurrentTemperature <= temp.HeatDamageThreshold)
                return hotMulti;

            _popup.PopupEntity(Loc.GetString("silicon-overheating"), silicon, silicon, PopupType.MediumCaution);
            return hotMulti;
        }

        if (temp.CurrentTemperature < thermal.NormalBodyTemperature)
            return 0.5f + temp.CurrentTemperature / thermal.NormalBodyTemperature * 0.5f;

        return 0;
    }
}
