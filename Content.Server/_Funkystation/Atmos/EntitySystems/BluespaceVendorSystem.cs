using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Content.Server._Funkystation.Atmos.Components;

namespace Content.Server._Funkystation.Atmos.EntitySystems;
/// <summary>
/// Contains all the server-side logic for BluespaceVendors.
/// <seealso cref="BluespaceVendorComponent"/>
/// </summary>
public sealed class BluespaceVendorSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceVendorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BluespaceVendorComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorHoldingTankEjectMessage>(OnTankEject);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorHoldingTankEmptyMessage>(OnTankEmpty);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorFillTankMessage>(OnTankFill);
        SubscribeLocalEvent<BluespaceVendorComponent, BluespaceVendorChangeReleasePressureMessage>(OnChangeReleasePressures);
        SubscribeLocalEvent<BluespaceVendorComponent, EntInsertedIntoContainerMessage>(OnTankInserted);
        SubscribeLocalEvent<BluespaceVendorComponent, EntRemovedFromContainerMessage>(OnTankRemoved);
    }

    private void OnStartup(EntityUid uid, BluespaceVendorComponent vendor, ComponentStartup args)
    {
        // Ensure container
        _slots.AddItemSlot(uid, vendor.TankContainerName, vendor.GasTankSlot);
        DirtyUI(uid, vendor);
    }

    private void OnUIOpened(EntityUid uid, BluespaceVendorComponent vendor, BoundUIOpenedEvent args)
    {
        DirtyUI(uid, vendor);
    }

    private void OnChangeReleasePressures(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorChangeReleasePressureMessage args)
    {
        vendor.ReleasePressures[args.Index] = args.Pressure;
        DirtyUI(uid, vendor);
    }

    private void DirtyUI(EntityUid uid,
        BluespaceVendorComponent? vendor = null)
    {
        if (!Resolve(uid, ref vendor))
            return;

        string? tankLabel = null;
        var tankPressure = 0f;

        if (TryGetGasTank(vendor, out var gasTank)  && gasTank != null)
        {
            tankLabel = Name(gasTank.Owner);
            tankPressure = gasTank.Air.Pressure;
        }

        if (GetEnabledGasList(uid, out List<bool>? senderEnabledList))
            vendor.BluespaceSenderEnabledList = senderEnabledList;

        _ui.SetUiState(uid, BluespaceVendorUiKey.Key,
            new BluespaceVendorBoundUserInterfaceState(Name(uid), tankLabel, tankPressure, vendor.ReleasePressures,
                vendor.MinReleasePressure, vendor.MaxReleasePressure, vendor.BluespaceGasMixture, 
                vendor.TankGasMixture, vendor.BluespaceSenderConnected, vendor.BluespaceSenderEnabledList));
    }

    private void OnTankFill(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorFillTankMessage args)
    {
        if (!vendor.BluespaceSenderConnected)
            return;

        if (!TryGetGasTank(vendor, out var gasTank))
            return;

        var mixture = gasTank!.Air;
        var releasePressure = vendor.ReleasePressures[args.Index];
        var maxReleasePressure = vendor.MaxReleasePressure;

        if (mixture.Pressure + releasePressure > maxReleasePressure) 
            releasePressure = Math.Max(maxReleasePressure - mixture.Pressure, 0f);
            
        var moles = (releasePressure * mixture.Volume) / (Atmospherics.R * Atmospherics.T20C);
        
        if (moles > vendor.BluespaceGasMixture.GetMoles(args.Index))
            moles = vendor.BluespaceGasMixture.GetMoles(args.Index);

        mixture.AdjustMoles(args.Index, moles);
        vendor.BluespaceGasMixture.AdjustMoles(args.Index, -moles);
        
        DirtyUI(uid, vendor);
    }

    private void OnTankInserted(EntityUid uid, BluespaceVendorComponent vendor, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != vendor.TankContainerName)
            return;

        if (vendor.GasTankSlot.Item != null)
        {
            var gasTank = Comp<GasTankComponent>(vendor.GasTankSlot.Item.Value);
            vendor.TankGasMixture = gasTank.Air;
            DirtyUI(uid, vendor);
        }
    }

    private void OnTankRemoved(EntityUid uid, BluespaceVendorComponent vendor, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != vendor.TankContainerName)
            return;

        DirtyUI(uid, vendor);
    }

    private void OnTankEject(EntityUid uid, BluespaceVendorComponent vendor, BluespaceVendorHoldingTankEjectMessage args)
    {
        if (vendor.GasTankSlot.Item == null)
            return;

        var item = vendor.GasTankSlot.Item;
        _slots.TryEjectToHands(uid, vendor.GasTankSlot, args.Actor);
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

    // Need to clean this up
    public bool GetEnabledGasList(EntityUid vendorUid, out List<bool> enabledList)
    {
        enabledList = new List<bool>();

        if (!TryComp(vendorUid, out BluespaceGasUtilizerComponent? utilizerComp))
            return false;
            
        var senderUid = utilizerComp.BluespaceSender;

        if (senderUid == null)
            return false;

        if (TryComp(senderUid.Value, out BluespaceSenderComponent? senderComp))
            enabledList = senderComp.BluespaceSenderEnabledList;

        return true;
    }

    private bool TryGetGasTank(BluespaceVendorComponent vendor, out GasTankComponent? gasTank)
    {
        gasTank = null;
        return vendor.GasTankSlot.Item.HasValue && TryComp(vendor.GasTankSlot.Item.Value, out gasTank);
    }
}
