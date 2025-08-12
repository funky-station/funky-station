// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Content.Shared.Database;
using Robust.Shared.Player;
using System.Linq;
using Content.Server._Funkystation.Atmos.Components;

namespace Content.Server._Funkystation.Atmos.EntitySystems;
/// <summary>
/// Contains all the server-side logic for bluespace vendors.
/// <seealso cref="BluespaceVendorComponent"/>
/// </summary>
public sealed class BluespaceVendorSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private TimeSpan _lastUpdateTime = TimeSpan.Zero;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(0.5);
    private List<int> _dangerousGases = [7, 8, 9];
    private List<int> _explosiveGases = [3, 4, 11, 13];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceVendorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BluespaceVendorComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorHoldingTankEjectMessage>(OnTankEject);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorHoldingTankEmptyMessage>(OnTankEmpty);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorChangeRetrieveMessage>(OnChangeRetrieve);
        SubscribeLocalEvent<BluespaceVendorComponent, EntInsertedIntoContainerMessage>(OnTankInserted);
        SubscribeLocalEvent<BluespaceVendorComponent, EntRemovedFromContainerMessage>(OnTankRemoved);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorChangeReleasePressureMessage>(OnChangeReleasePressure);
        SubscribeLocalEvent<BluespaceVendorComponent, AtmosDeviceUpdateEvent>(OnDeviceAtmosUpdate);
    }

    private void OnStartup(EntityUid uid, BluespaceVendorComponent vendor, ComponentStartup args)
    {
        // Ensure container
        _slots.AddItemSlot(uid, vendor.TankContainerName, vendor.GasTankSlot);

        // Check for linked sender
        if (TryComp<BluespaceGasUtilizerComponent>(uid, out var utilizer) && utilizer.BluespaceSender != null)
        {
            if (TryComp<BluespaceSenderComponent>(utilizer.BluespaceSender.Value, out var sender))
            {
                vendor.BluespaceGasMixture = sender.BluespaceGasMixture;
                vendor.BluespaceSenderConnected = true;
            }
        }

        DirtyUI(uid, vendor);
    }

    private void OnUIOpened(EntityUid uid, BluespaceVendorComponent vendor, BoundUIOpenedEvent args)
    {
        DirtyUI(uid, vendor);
    }

    private void OnChangeRetrieve(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorChangeRetrieveMessage args)
    {
        int index = args.Index;

        if (_explosiveGases.Contains(index) && !vendor.BluespaceVendorRetrieveList[index])
        {
            _adminLogger.Add(LogType.Explosion, LogImpact.Medium, $"Player {ToPrettyString(args.Actor):player} potentially dispensing {Enum.GetName(typeof(Gas), index)} from {ToPrettyString(uid):vendor} into tank");
        }

        vendor.BluespaceVendorRetrieveList[index] = !vendor.BluespaceVendorRetrieveList[index];
        DirtyUI(uid, vendor);
    }

    private void OnChangeReleasePressure(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorChangeReleasePressureMessage args)
    {
        var pressure = args.Pressure;
        if (pressure > 100f) pressure = 100f;
        if (pressure < 0f) pressure = 0f;
        vendor.ReleasePressure = pressure;
        DirtyUI(uid, vendor);
    }

    private void OnDeviceAtmosUpdate(EntityUid uid, BluespaceVendorComponent vendor, ref AtmosDeviceUpdateEvent args)
    {
        if (!_power.IsPowered(uid) || !TryComp<ApcPowerReceiverComponent>(uid, out _))
        {
            UpdatePumpingAppearance(uid, vendor, false);
            return;
        }

        var retrieveList = vendor.BluespaceVendorRetrieveList;
        var isRetrievingCount = retrieveList.Count(x => x);
        var bluespaceMixture = vendor.BluespaceGasMixture;
        var tankMixture = vendor.TankGasMixture;
        var releasePressure = vendor.ReleasePressure * 10;

        if (isRetrievingCount < 1 || releasePressure - tankMixture.Pressure < 0.001f || vendor.GasTankSlot.Item == null)
        {
            UpdatePumpingAppearance(uid, vendor, false);
            return;
        }

        UpdatePumpingAppearance(uid, vendor, true);
        
        // When retrieving, add selected gases from bluespace to tank
        var initMoles = Math.Min(((releasePressure * tankMixture.Volume) / (Atmospherics.R * Atmospherics.T20C) - tankMixture.TotalMoles), 0.2f) / isRetrievingCount;

        for (var i = 0; i < retrieveList.Count; i++)
        {
            if (!retrieveList[i])
                continue;

            var moles = initMoles;
            moles = moles >= bluespaceMixture.GetMoles(i) ? bluespaceMixture.GetMoles(i) : moles;
            
            bluespaceMixture.AdjustMoles(i, -moles);
            if (bluespaceMixture.GetMoles(i) < 0.01f)
                vendor.BluespaceVendorRetrieveList[i] = !vendor.BluespaceVendorRetrieveList[i];
                
            var addedGases = new GasMixture();
            addedGases.AdjustMoles(i, moles);
            addedGases.Temperature = Atmospherics.T20C;
            _atmos.Merge(tankMixture, addedGases);
        }

        DirtyUI(uid, vendor);
    }

    private void DirtyUI(EntityUid uid,
        BluespaceVendorComponent? vendor = null)
    {
        if (!Resolve(uid, ref vendor) || !HasComp<MetaDataComponent>(uid))
            return;

        string? tankLabel = null;

        if (TryGetGasTank(vendor, out var gasTank)  && gasTank != null)
        {
            tankLabel = Name(gasTank.Owner);
        }

        _ui.SetUiState(uid, BluespaceVendorUiKey.Key,
            new BluespaceVendorBoundUserInterfaceState(Name(uid), tankLabel, vendor.BluespaceVendorRetrieveList,
                vendor.BluespaceGasMixture, vendor.TankGasMixture, vendor.ReleasePressure, vendor.BluespaceSenderConnected));
    }

    private void OnTankInserted(EntityUid uid, BluespaceVendorComponent vendor, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != vendor.TankContainerName)
            return;

        if (vendor.GasTankSlot.Item != null)
        {   
            UpdateTankAppearance(uid, vendor);
            var gasTank = Comp<GasTankComponent>(vendor.GasTankSlot.Item.Value);
            vendor.TankGasMixture = gasTank.Air;
            DirtyUI(uid, vendor);
        }
    }

    private void OnTankRemoved(EntityUid uid, BluespaceVendorComponent vendor, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != vendor.TankContainerName)
            return;
        
        HandleTankRemoval(uid, vendor);
    }

    private void OnTankEject(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorHoldingTankEjectMessage args)
    {
        if (vendor.GasTankSlot.Item == null)
            return;

        _slots.TryEjectToHands(uid, vendor.GasTankSlot, args.Actor);
        HandleTankRemoval(uid, vendor);
    }

    private void HandleTankRemoval(EntityUid uid, BluespaceVendorComponent vendor)
    {
        vendor.TankGasMixture = new();
        UpdateTankAppearance(uid, vendor);
        DirtyUI(uid, vendor);
    }

    private void OnTankEmpty(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorHoldingTankEmptyMessage args)
    {
        if (vendor.GasTankSlot.Item == null)
            return;

        if (!vendor.BluespaceSenderConnected)
            return;
        
        var gasTank = Comp<GasTankComponent>(vendor.GasTankSlot.Item.Value);
        var mixture = gasTank.Air;
        var removedAir = mixture.Remove(mixture.TotalMoles);
        
        _atmos.Merge(vendor.BluespaceGasMixture, removedAir);
        DirtyUI(uid, vendor);
    }

    public void OnBluespaceSenderConnected(EntityUid uid, BluespaceVendorComponent vendor)
    {
        DirtyUI(uid, vendor);
    }

    private bool TryGetGasTank(BluespaceVendorComponent vendor, out GasTankComponent? gasTank)
    {
        gasTank = null;
        return vendor.GasTankSlot.Item.HasValue && TryComp(vendor.GasTankSlot.Item.Value, out gasTank);
    }

    private void UpdateTankAppearance(EntityUid uid, BluespaceVendorComponent vendor, GasTankComponent? gasTank = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance, false))
            return;
        bool tankInserted = vendor.GasTankSlot.Item.HasValue && TryComp(vendor.GasTankSlot.Item.Value, out GasTankComponent? _);
        if (!tankInserted)
        {
            UpdatePumpingAppearance(uid, vendor, false);
        }
        _appearance.SetData(uid, BluespaceVendorVisuals.TankInserted, tankInserted, appearance);
    }

    private void UpdatePumpingAppearance(EntityUid uid, BluespaceVendorComponent vendor, bool isPumping = false, GasTankComponent? gasTank = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, BluespaceVendorVisuals.isPumping, isPumping, appearance);
    }
}
