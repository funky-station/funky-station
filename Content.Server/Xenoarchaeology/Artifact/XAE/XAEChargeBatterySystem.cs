// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Xenoarchaeology.Artifact.XAE.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.XAE;

namespace Content.Server.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// System for xeno artifact activation effect that is fully charging batteries in certain range.
/// </summary>
public sealed class XAEChargeBatterySystem : BaseXAESystem<XAEChargeBatteryComponent>
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly PredictedBatterySystem _predictedBattery = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    /// <summary> Pre-allocated and re-used collection.</summary>
    private readonly HashSet<Entity<BatteryComponent>> _batteryEntities = new();
    private readonly HashSet<Entity<PredictedBatteryComponent>> _pBatteryEntities = new();

    /// <inheritdoc />
    protected override void OnActivated(Entity<XAEChargeBatteryComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        _batteryEntities.Clear();
        _pBatteryEntities.Clear();

        _lookup.GetEntitiesInRange(args.Coordinates, ent.Comp.Radius, _batteryEntities);
        foreach (var battery in _batteryEntities)
        {
            _battery.SetCharge(battery, battery.Comp.MaxCharge, battery);
        }

        _lookup.GetEntitiesInRange(args.Coordinates, ent.Comp.Radius, _pBatteryEntities);
        foreach (var pBattery in _pBatteryEntities)
        {
            _predictedBattery.SetCharge(pBattery.AsNullable(), pBattery.Comp.MaxCharge);
        }
    }
}
