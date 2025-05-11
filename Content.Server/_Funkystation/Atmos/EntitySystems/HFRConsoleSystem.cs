using Content.Shared._Funkystation.Atmos.Components;
using Content.Server._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.UserInterface;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Interaction;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HFRCoreSystem _coreSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly HypertorusFusionReactorSystem _hfrSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HFRConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleTogglePowerMessage>(OnTogglePowerMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleToggleCoolingMessage>(OnToggleCoolingMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleToggleFuelInjectionMessage>(OnToggleFuelInjectionMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleToggleModeratorInjectionMessage>(OnToggleModeratorInjectionMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetFuelInputRateMessage>(OnSetFuelInputRateMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetModeratorInputRateMessage>(OnSetModeratorInputRateMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSelectRecipeMessage>(OnSelectRecipeMessage);
        SubscribeLocalEvent<HFRConsoleComponent, ComponentStartup>(OnConsoleStartup);
        SubscribeLocalEvent<HFRConsoleComponent, AnchorStateChangedEvent>(OnConsoleAnchorChanged);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleToggleWasteRemoveMessage>(OnToggleWasteRemoveMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetHeatingConductorMessage>(OnSetHeatingConductorMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetCoolingVolumeMessage>(OnSetCoolingVolumeMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetMagneticConstrictorMessage>(OnSetMagneticConstrictorMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetCurrentDamperMessage>(OnSetCurrentDamperMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetModeratorFilteringRateMessage>(OnSetModeratorFilteringRateMessage);
        SubscribeLocalEvent<HFRConsoleComponent, HFRConsoleSetFilterGasesMessage>(OnSetFilterGasesMessage);
        SubscribeLocalEvent<HFRConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
    }

    private void OnComponentInit(Entity<HFRConsoleComponent> ent, ref ComponentInit args)
    {
        SetPowerState(ent, ent.Comp);
    }

    private void OnBeforeOpened(Entity<HFRConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent, ent.Comp);
    }

    private void OnConsoleStartup(EntityUid uid, HFRConsoleComponent console, ComponentStartup args)
    {
        TryFindCore(uid, console);
        SetPowerState(uid, console);
    }

    private void OnConsoleAnchorChanged(EntityUid uid, HFRConsoleComponent console, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (console.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(console.CoreUid, out var coreComp))
                {
                    coreComp.ConsoleUid = null;
                    _hfrSystem.ToggleActiveState(console.CoreUid.Value, coreComp, false);
                }
                console.CoreUid = null;
            }
            SetPowerState(uid, console);
        }
        else
        {
            TryFindCore(uid, console);
            SetPowerState(uid, console);
        }
    }

    private void TryFindCore(EntityUid uid, HFRConsoleComponent console)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var rotation = xform.LocalRotation;
        var direction = rotation.GetCardinalDir();
        var offset = -direction.ToIntVec();
        var consoleCoords = _transformSystem.GetMapCoordinates(uid);
        var consoleTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, consoleCoords);
        var targetTile = consoleTile + offset;

        var coreQuery = GetEntityQuery<HFRCoreComponent>();
        bool foundCore = false;
        foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile))
        {
            if (coreQuery.TryGetComponent(entity, out var coreComp))
            {
                _coreSystem.TryLinkComponent(entity, coreComp, uid, console, (core, compUid) => core.ConsoleUid = compUid);
                foundCore = true;
                break;
            }
        }

        SetPowerState(uid, console);
    }

    public void SetPowerState(EntityUid uid, HFRConsoleComponent console)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
            return;

        bool shouldBePowered = false;
        if (console.CoreUid != null && 
            EntityManager.EntityExists(console.CoreUid.Value) &&
            TryComp<HFRCoreComponent>(console.CoreUid.Value, out var coreComp))
        {
            shouldBePowered = _hfrSystem.AreAllPartsConnected(console.CoreUid.Value, coreComp);
        }

        powerReceiver.PowerDisabled = !shouldBePowered;
    }

    private void OnTogglePowerMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleTogglePowerMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            if (coreComp.IsActive && coreComp.PowerLevel > 0)
            {
                DirtyUI(uid, component);
                return;
            }
            _hfrSystem.ToggleActiveState(component.CoreUid.Value, coreComp, !coreComp.IsActive);
            DirtyUI(uid, component);
        }
    }

    private void OnToggleCoolingMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleToggleCoolingMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            if (coreComp.IsActive && !coreComp.IsFuelInjecting && !coreComp.IsModeratorInjecting)
            {
                coreComp.IsCooling = !coreComp.IsCooling;
            }
            DirtyUI(uid, component);
        }
    }

    private void OnToggleFuelInjectionMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleToggleFuelInjectionMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            if (coreComp.IsActive && coreComp.IsCooling)
            {
                coreComp.IsFuelInjecting = !coreComp.IsFuelInjecting;
            }
            DirtyUI(uid, component);
        }
    }

    private void OnToggleModeratorInjectionMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleToggleModeratorInjectionMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            if (coreComp.IsActive && coreComp.IsCooling)
            {
                coreComp.IsModeratorInjecting = !coreComp.IsModeratorInjecting;
            }
            DirtyUI(uid, component);
        }
    }

    private void OnSetFuelInputRateMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetFuelInputRateMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.FuelInputRate = Math.Clamp(args.Rate, 0.5f, 150f);
            DirtyUI(uid, component);
        }
    }

    private void OnSetModeratorInputRateMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetModeratorInputRateMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.ModeratorInputRate = Math.Clamp(args.Rate, 0.5f, 150f);
            DirtyUI(uid, component);
        }
    }

    private void OnSelectRecipeMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSelectRecipeMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.SelectedRecipeId = args.RecipeId;
            DirtyUI(uid, component);
        }
    }

    private void OnToggleWasteRemoveMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleToggleWasteRemoveMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.IsWasteRemoving = !coreComp.IsWasteRemoving;
            DirtyUI(uid, component);
        }
    }

    private void OnSetHeatingConductorMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetHeatingConductorMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.HeatingConductor = Math.Clamp(args.Rate, 50f, 500f);
            DirtyUI(uid, component);
        }
    }

    private void OnSetCoolingVolumeMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetCoolingVolumeMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.CoolingVolume = Math.Clamp(args.Rate, 50, 2000);
            DirtyUI(uid, component);
        }
    }

    private void OnSetMagneticConstrictorMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetMagneticConstrictorMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.MagneticConstrictor = Math.Clamp(args.Rate, 50f, 1000f);
            DirtyUI(uid, component);
        }
    }

    private void OnSetCurrentDamperMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetCurrentDamperMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.CurrentDamper = Math.Clamp(args.Rate, 0f, 1000f);
            DirtyUI(uid, component);
        }
    }

    private void OnSetModeratorFilteringRateMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetModeratorFilteringRateMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.ModeratorFilteringRate = Math.Clamp(args.Rate, 5f, 200f);
            DirtyUI(uid, component);
        }
    }

    private void OnSetFilterGasesMessage(EntityUid uid, HFRConsoleComponent component, HFRConsoleSetFilterGasesMessage args)
    {
        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            coreComp.FilterGases = new HashSet<Gas>(args.Gases);
            DirtyUI(uid, component);
        }
    }

    private void DirtyUI(EntityUid uid, HFRConsoleComponent? component, UserInterfaceComponent? ui = null)
    {
        if (!Resolve(uid, ref component, ref ui, false))
            return;

        bool isActive = false;
        bool isCooling = false;
        bool isFuelInjecting = false;
        bool isModeratorInjecting = false;
        float fuelInputRate = 0f;
        float moderatorInputRate = 0f;
        string? selectedRecipeId = null;
        float heatingConductor = 0f;
        int coolingVolume = 0;
        float magneticConstrictor = 0f;
        float currentDamper = 0f;
        bool isWasteRemoving = false;
        float moderatorFilteringRate = 0f;
        HashSet<Gas> filterGases = new();
        Dictionary<Gas, float> internalFusionMoles = new();
        Dictionary<Gas, float> moderatorInternalMoles = new();
        float criticalThresholdProximity = 0f;
        float meltingPoint = 900f;
        float ironContent = 0f;
        float powerOutput = 0f;
        int powerLevel = 0;
        float energy = 0f;
        float efficiency = 0f;
        float instability = 0f;
        float fusionTemperature = 3f;
        float fusionTemperatureArchived = 3f;
        float moderatorTemperature = 3f;
        float moderatorTemperatureArchived = 3f;
        float coolantTemperature = 0f;
        float coolantTemperatureArchived = 0f;
        float outputTemperature = 0f;
        float outputTemperatureArchived = 0f;
        float coolantMoles = 0f;
        float outputMoles = 0f;

        if (component.CoreUid != null && TryComp<HFRCoreComponent>(component.CoreUid, out var coreComp))
        {
            isActive = coreComp.IsActive;
            isCooling = coreComp.IsCooling;
            isFuelInjecting = coreComp.IsFuelInjecting;
            isModeratorInjecting = coreComp.IsModeratorInjecting;
            fuelInputRate = coreComp.FuelInputRate;
            moderatorInputRate = coreComp.ModeratorInputRate;
            selectedRecipeId = coreComp.SelectedRecipeId;
            heatingConductor = coreComp.HeatingConductor;
            coolingVolume = coreComp.CoolingVolume;
            magneticConstrictor = coreComp.MagneticConstrictor;
            currentDamper = coreComp.CurrentDamper;
            isWasteRemoving = coreComp.IsWasteRemoving;
            moderatorFilteringRate = coreComp.ModeratorFilteringRate;
            filterGases = new HashSet<Gas>(coreComp.FilterGases);

            if (coreComp.InternalFusion != null)
            {
                foreach (Gas gas in Enum.GetValues(typeof(Gas)))
                {
                    float moles = coreComp.InternalFusion.GetMoles(gas);
                    if (moles > 0)
                        internalFusionMoles[gas] = moles;
                }
            }
            if (coreComp.ModeratorInternal != null)
            {
                foreach (Gas gas in Enum.GetValues(typeof(Gas)))
                {
                    float moles = coreComp.ModeratorInternal.GetMoles(gas);
                    if (moles > 0)
                        moderatorInternalMoles[gas] = moles;
                }
            }

            criticalThresholdProximity = coreComp.CriticalThresholdProximity;
            meltingPoint = coreComp.MeltingPoint;
            ironContent = coreComp.IronContent;
            powerOutput = coreComp.PowerOutput;
            powerLevel = coreComp.PowerLevel;
            energy = coreComp.Energy;
            efficiency = coreComp.Efficiency;
            instability = coreComp.Instability;
            fusionTemperature = coreComp.FusionTemperature;
            fusionTemperatureArchived = coreComp.FusionTemperatureArchived;
            moderatorTemperature = coreComp.ModeratorTemperature;
            moderatorTemperatureArchived = coreComp.ModeratorTemperatureArchived;

            // Calculate coolant moles and temperature from HFR core pipe
            if (_nodeContainer.TryGetNode(component.CoreUid.Value, "pipe", out PipeNode? corePipe) && corePipe.Air != null)
            {
                coolantMoles = corePipe.Air.TotalMoles;
                coolantTemperature = corePipe.Air.Temperature;
                coolantTemperatureArchived = coolantTemperature; 
            }

            // Calculate output moles and temperature from HFRWasteOutput pipe
            if (coreComp.WasteOutputUid != null &&
                _nodeContainer.TryGetNode(coreComp.WasteOutputUid.Value, "pipe", out PipeNode? wastePipe) &&
                wastePipe.Air != null)
            {
                outputMoles = wastePipe.Air.TotalMoles;
                outputTemperature = wastePipe.Air.Temperature;
                outputTemperatureArchived = outputTemperature; 
            }
        }

        _userInterfaceSystem.SetUiState(uid, HFRConsoleUiKey.Key,
            new HFRConsoleBoundInterfaceState(
                isActive,
                isCooling,
                isFuelInjecting,
                isModeratorInjecting,
                fuelInputRate,
                moderatorInputRate,
                selectedRecipeId,
                heatingConductor,
                coolingVolume,
                magneticConstrictor,
                currentDamper,
                isWasteRemoving,
                moderatorFilteringRate,
                filterGases,
                internalFusionMoles,
                moderatorInternalMoles,
                criticalThresholdProximity,
                meltingPoint,
                ironContent,
                powerOutput,
                powerLevel,
                energy,
                efficiency,
                instability,
                fusionTemperature,
                fusionTemperatureArchived,
                moderatorTemperature,
                moderatorTemperatureArchived,
                coolantTemperature,
                coolantTemperatureArchived,
                outputTemperature,
                outputTemperatureArchived,
                coolantMoles,
                outputMoles));
    }
}