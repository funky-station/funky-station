using Content.Server._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRFuelInputSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HFRCoreSystem _coreSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly HypertorusFusionReactorSystem _hfrSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HFRFuelInputComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HFRFuelInputComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnStartup(EntityUid uid, HFRFuelInputComponent fuelInput, ComponentStartup args)
    {
        TryFindCore(uid, fuelInput);
    }

    private void OnAnchorChanged(EntityUid uid, HFRFuelInputComponent fuelInput, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (fuelInput.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(fuelInput.CoreUid, out var coreComp))
                {
                    fuelInput.IsActive = false;
                    if (TryComp<AppearanceComponent>(uid, out var appearance))
                    {
                        _appearanceSystem.SetData(uid, HFRVisuals.IsActive, false, appearance);
                    }

                    coreComp.FuelInputUid = null;
                    _hfrSystem.ToggleActiveState(fuelInput.CoreUid.Value, coreComp, false);
                }
                fuelInput.CoreUid = null;
            }
        }
        else
        {
            TryFindCore(uid, fuelInput);
        }
    }

    private void TryFindCore(EntityUid uid, HFRFuelInputComponent fuelInput)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var rotation = xform.LocalRotation;
        var direction = rotation.GetCardinalDir();
        var offset = -direction.ToIntVec();
        var fuelInputCoords = _transformSystem.GetMapCoordinates(uid);
        var fuelInputTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, fuelInputCoords);
        var targetTile = fuelInputTile + offset;

        var coreQuery = GetEntityQuery<HFRCoreComponent>();
        foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile))
        {
            if (coreQuery.TryGetComponent(entity, out var coreComp))
            {
                _coreSystem.TryLinkComponent(entity, coreComp, uid, fuelInput, (core, compUid) => core.FuelInputUid = compUid);
                break;
            }
        }
    }
}