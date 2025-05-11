using Content.Server._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRWasteOutputSystem : EntitySystem
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
        SubscribeLocalEvent<HFRWasteOutputComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HFRWasteOutputComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnStartup(EntityUid uid, HFRWasteOutputComponent wasteOutput, ComponentStartup args)
    {
        TryFindCore(uid, wasteOutput);
    }

    private void OnAnchorChanged(EntityUid uid, HFRWasteOutputComponent wasteOutput, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (wasteOutput.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(wasteOutput.CoreUid, out var coreComp))
                {
                    wasteOutput.IsActive = false;
                    if (TryComp<AppearanceComponent>(uid, out var appearance))
                    {
                        _appearanceSystem.SetData(uid, HFRVisuals.IsActive, false, appearance);
                    }

                    coreComp.WasteOutputUid = null;
                    _hfrSystem.ToggleActiveState(wasteOutput.CoreUid.Value, coreComp, false);
                }
                wasteOutput.CoreUid = null;
            }
        }
        else
        {
            TryFindCore(uid, wasteOutput);
        }
    }

    private void TryFindCore(EntityUid uid, HFRWasteOutputComponent wasteOutput)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var rotation = xform.LocalRotation;
        var direction = rotation.GetCardinalDir();
        var offset = -direction.ToIntVec();
        var wasteOutputCoords = _transformSystem.GetMapCoordinates(uid);
        var wasteOutputTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, wasteOutputCoords);
        var targetTile = wasteOutputTile + offset;

        var coreQuery = GetEntityQuery<HFRCoreComponent>();
        foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile))
        {
            if (coreQuery.TryGetComponent(entity, out var coreComp))
            {
                _coreSystem.TryLinkComponent(entity, coreComp, uid, wasteOutput, (core, compUid) => core.WasteOutputUid = compUid);
                break;
            }
        }
    }
}