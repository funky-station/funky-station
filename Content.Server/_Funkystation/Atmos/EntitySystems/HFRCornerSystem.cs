using Content.Server._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRCornerSystem : EntitySystem
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
        SubscribeLocalEvent<HFRCornerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HFRCornerComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnStartup(EntityUid uid, HFRCornerComponent corner, ComponentStartup args)
    {
        TryFindCore(uid, corner);
    }

    private void OnAnchorChanged(EntityUid uid, HFRCornerComponent corner, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (corner.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(corner.CoreUid, out var coreComp))
                {
                    corner.IsActive = false;
                    if (TryComp<AppearanceComponent>(uid, out var appearance))
                    {
                        _appearanceSystem.SetData(uid, HFRVisuals.IsActive, false, appearance);
                    }

                    coreComp.CornerUids.Remove(uid);
                    _hfrSystem.ToggleActiveState(corner.CoreUid.Value, coreComp, false);
                }
                corner.CoreUid = null;
            }
        }
        else
        {
            TryFindCore(uid, corner);
        }
    }

    private void TryFindCore(EntityUid uid, HFRCornerComponent corner)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var cornerCoords = _transformSystem.GetMapCoordinates(uid);
        var cornerTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, cornerCoords);

        var coreQuery = GetEntityQuery<HFRCoreComponent>();
        foreach (var offset in HFRCoreSystem.DiagonalOffsets)
        {
            var targetTile = cornerTile + offset;
            foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile))
            {
                if (coreQuery.TryGetComponent(entity, out var coreComp))
                {
                    _coreSystem.TryLinkCorner(entity, coreComp, uid, corner);
                    break;
                }
            }
        }
    }
}