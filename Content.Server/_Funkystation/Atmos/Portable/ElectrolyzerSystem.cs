// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

// Assmos - /tg/ gases
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Shared.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Server.Stack;
using Content.Server.Hands.Systems;
using Content.Shared.Tag;
using Content.Shared.Hands.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server._Funkystation.Atmos.Portable;

public sealed class ElectrolyzerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    private const float WorkingPower = 2f;
    private const float PowerEfficiency = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElectrolyzerComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<ElectrolyzerComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<ElectrolyzerComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ElectrolyzerComponent, InteractUsingEvent>(OnInteractUsingFuel);
    }

    private void OnSignalReceived(EntityUid uid, ElectrolyzerComponent comp, SignalReceivedEvent args)
    {
        if (!TryComp<DeviceLinkSinkComponent>(uid, out _))
            return;

        bool? newState = null;

        switch (args.Port)
        {
            case "On":
                newState = true;
                break;
            case "Off":
                newState = false;
                break;
            case "Toggle":
                newState = !comp.IsPowered;
                break;
            default:
                return;
        }

        if (newState == comp.IsPowered)
            return;

        comp.IsPowered = newState.Value;

        UpdateAppearance(uid);
    }

    private void OnActivate(EntityUid uid, ElectrolyzerComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled) return;

        if (comp.IsPowered)
        {
            comp.IsPowered = false;
            _popup.PopupEntity("electrolyzer-turned-off", uid, args.User);
        }
        else
        {
            // Turn on only if anchored and has fuel (or fuel in slot)
            if (!Transform(uid).Anchored)
            {
                _popup.PopupEntity("electrolyzer-must-be-anchored", uid, args.User);
                return;
            }

            bool hasFuel = comp.CurrentFuel > 0f ||
                           (_itemSlots.TryGetSlot(uid, "fuel", out var slot) &&
                            slot.ContainerSlot?.ContainedEntity != null);

            if (!hasFuel)
            {
                _popup.PopupEntity("electrolyzer-no-fuel", uid, args.User);
                return;
            }

            comp.IsPowered = true;
            _popup.PopupEntity("electrolyzer-turned-on", uid, args.User);
        }

        UpdateAppearance(uid);
        args.Handled = true;
    }

    private void UpdateAppearance(EntityUid uid)
    {
        if (EntityManager.TryGetComponent<ElectrolyzerComponent>(uid, out var comp))
        {
            _appearance.SetData(uid, ElectrolyzerVisuals.State,
                comp.IsPowered ? ElectrolyzerState.On : ElectrolyzerState.Off);
        }
    }

    private void OnDeviceUpdated(EntityUid uid, ElectrolyzerComponent electrolyzer, ref AtmosDeviceUpdateEvent args)
    {
        if (!Transform(uid).Anchored || !electrolyzer.IsPowered)
            return;

        if (electrolyzer.CurrentFuel <= 0f)
        {
            if (!_itemSlots.TryGetSlot(uid, "fuel", out var slot) || slot.ContainerSlot?.ContainedEntity is not { } fuelEntity)
            {
                electrolyzer.IsPowered = false; // auto-shutdown if no more fuel possible
                UpdateAppearance(uid);
                _popup.PopupEntity("electrolyzer-no-fuel", uid);
                return;
            }

            if (!TryComp<StackComponent>(fuelEntity, out var stack) || stack.Count <= 0)
            {
                electrolyzer.IsPowered = false;
                UpdateAppearance(uid);
                _popup.PopupEntity("electrolyzer-no-fuel", uid);
                return;
            }

            // Determine fuel value per sheet
            float fuelPerSheet = 0f;
            if (_tagSystem.HasTag(fuelEntity, "PlasmaSheet"))
                fuelPerSheet = electrolyzer.PlasmaFuelConversion;
            else if (_tagSystem.HasTag(fuelEntity, "UraniumSheet"))
                fuelPerSheet = electrolyzer.UraniumFuelConversion;
            else
                return;

            // Consume 1 sheet
            _stackSystem.SetCount(fuelEntity, stack.Count - 1);
            electrolyzer.CurrentFuel = fuelPerSheet;

            // If stack now empty, delete it
            if (stack.Count <= 0)
                EntityManager.QueueDeleteEntity(fuelEntity);
        }

        UpdateAppearance(uid);

        var mixture = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);
        if (mixture is null) return;

        var initH2O = mixture.GetMoles(Gas.WaterVapor);
        var initHyperNob = mixture.GetMoles(Gas.HyperNoblium);
        var initBZ = mixture.GetMoles(Gas.BZ);
        var temperature = mixture.Temperature;
        float powerLoad = 100f;
        float activeLoad = (4200f * (3f * WorkingPower) * WorkingPower) / (PowerEfficiency + WorkingPower);

        if (initH2O > 0.05f)
        {
            var maxProportion = 2.5f * (float) Math.Pow(WorkingPower, 2);
            var proportion = Math.Min(initH2O * 0.5f, maxProportion);
            var temperatureEfficiency = Math.Min(mixture.Temperature / 1123.15f, 1f);

            var h2oRemoved = proportion * 2f;
            var oxyProduced = proportion * temperatureEfficiency;
            var hydrogenProduced = proportion * 2f * temperatureEfficiency;

            mixture.AdjustMoles(Gas.WaterVapor, -h2oRemoved);
            mixture.AdjustMoles(Gas.Oxygen, oxyProduced);
            mixture.AdjustMoles(Gas.Hydrogen, hydrogenProduced);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            powerLoad = Math.Max(activeLoad * (hydrogenProduced / (maxProportion * 2)), powerLoad);
        }

        if (initHyperNob > 0.01f && temperature < 150f)
        {
            var maxProportion = 1.5f * (float) Math.Pow(WorkingPower, 2);
            var proportion = Math.Min(initHyperNob, maxProportion);
            mixture.AdjustMoles(Gas.HyperNoblium, -proportion);
            mixture.AdjustMoles(Gas.AntiNoblium, proportion * 0.5f);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            powerLoad = Math.Max(activeLoad * (proportion / maxProportion), powerLoad);
        }

        if (initBZ > 0.01f)
        {
            var proportion = Math.Min(initBZ * (1f - (float) Math.Pow(Math.E, -0.5f * temperature * WorkingPower / Atmospherics.FireMinimumTemperatureToExist)), initBZ);
            mixture.AdjustMoles(Gas.BZ, -proportion);
            mixture.AdjustMoles(Gas.Oxygen, proportion * 0.2f);
            mixture.AdjustMoles(Gas.Halon, proportion * 2f);
            var energyReleased = proportion * Atmospherics.HalonProductionEnergy;

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            if (heatCap > Atmospherics.MinimumHeatCapacity)
                mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);
            powerLoad = Math.Max(activeLoad * Math.Min(proportion / 30f, 1), powerLoad);
        }

        const float fuelPerUnitWork = 0.1f;
        float fuelNeeded = powerLoad * fuelPerUnitWork;

        // Clamp to available fuel
        if (electrolyzer.CurrentFuel < fuelNeeded)
            electrolyzer.CurrentFuel = 0f;
        else
            electrolyzer.CurrentFuel -= fuelNeeded;

        _gasOverlaySystem.UpdateSessions();
    }

    private void OnInteractUsingFuel(EntityUid uid, ElectrolyzerComponent comp, InteractUsingEvent args)
    {
        if (args.Handled || args.Target != uid)
            return;

        if (!_itemSlots.TryGetSlot(uid, "fuel", out var slot) || slot.ContainerSlot == null)
            return;

        var heldItem = args.Used;
        var existingItem = slot.ContainerSlot.ContainedEntity;

        // Tag checks
        bool heldIsPlasma = _tagSystem.HasTag(heldItem, "PlasmaSheet");
        bool heldIsUranium = _tagSystem.HasTag(heldItem, "UraniumSheet");

        if (!heldIsPlasma && !heldIsUranium)
            return;

        args.Handled = true;

        if (existingItem == null)
        {
            // Empty: insert normally
            if (_itemSlots.TryInsert(uid, "fuel", heldItem, args.User))
            {
                _popup.PopupEntity("electrolyzer-fuel-inserted", uid, args.User);
            }
            return;
        }

        bool existingIsPlasma = _tagSystem.HasTag(existingItem.Value, "PlasmaSheet");
        bool existingIsUranium = _tagSystem.HasTag(existingItem.Value, "UraniumSheet");

        // Same type: merge
        if ((heldIsPlasma && existingIsPlasma) || (heldIsUranium && existingIsUranium))
        {
            if (!TryComp<StackComponent>(heldItem, out var heldStack) ||
                !TryComp<StackComponent>(existingItem.Value, out var existingStack))
            {
                _popup.PopupEntity("electrolyzer-cannot-merge-invalid-stack", uid, args.User); // Should never happen
                return;
            }

            int maxStack = _stackSystem.GetMaxCount(existingStack);
            int total = existingStack.Count + heldStack.Count;

            if (total > maxStack)
            {
                int toAdd = maxStack - existingStack.Count;
                _stackSystem.SetCount(existingItem.Value, maxStack);
                _stackSystem.SetCount(heldItem, heldStack.Count - toAdd);
            }
            else
            {
                _stackSystem.SetCount(existingItem.Value, total);
                EntityManager.QueueDeleteEntity(heldItem);
            }

            return;
        }

        // Different type: swap
        EntityUid? ejected;
        if (_itemSlots.TryEject(uid, "fuel", args.User, out ejected))
        {
            // Insert the new held item first
            if (_itemSlots.TryInsert(uid, "fuel", heldItem, args.User))
            {
                _popup.PopupEntity("electrolyzer-fuel-swapped", uid, args.User);

                if (ejected != null && args.User != null && TryComp<HandsComponent>(args.User, out var hands))
                {
                    var activeHand = hands.ActiveHand;
                    if (activeHand != null)
                    {
                        _handsSystem.TryPickup(args.User, ejected.Value, handName: activeHand.Name, handsComp: hands);
                    }
                    else
                    {
                        _handsSystem.PickupOrDrop(args.User, ejected.Value);
                    }
                }
            }
        }
    }
}
