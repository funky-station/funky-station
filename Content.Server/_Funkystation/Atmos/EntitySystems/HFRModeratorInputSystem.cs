using Content.Server._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRModeratorInputSystem : EntitySystem
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
        SubscribeLocalEvent<HFRModeratorInputComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HFRModeratorInputComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnStartup(EntityUid uid, HFRModeratorInputComponent moderatorInput, ComponentStartup args)
    {
        TryFindCore(uid, moderatorInput);
    }

    private void OnAnchorChanged(EntityUid uid, HFRModeratorInputComponent moderatorInput, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (moderatorInput.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(moderatorInput.CoreUid, out var coreComp))
                {
                    moderatorInput.IsActive = false;
                    if (TryComp<AppearanceComponent>(uid, out var appearance))
                    {
                        _appearanceSystem.SetData(uid, HFRVisuals.IsActive, false, appearance);
                    }

                    coreComp.ModeratorInputUid = null;
                    _hfrSystem.ToggleActiveState(moderatorInput.CoreUid.Value, coreComp, false);
                }
                moderatorInput.CoreUid = null;
            }
        }
        else
        {
            TryFindCore(uid, moderatorInput);
        }
    }

    private void TryFindCore(EntityUid uid, HFRModeratorInputComponent moderatorInput)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var rotation = xform.LocalRotation;
        var direction = rotation.GetCardinalDir();
        var offset = -direction.ToIntVec();
        var moderatorInputCoords = _transformSystem.GetMapCoordinates(uid);
        var moderatorInputTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, moderatorInputCoords);
        var targetTile = moderatorInputTile + offset;

        var coreQuery = GetEntityQuery<HFRCoreComponent>();
        foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile))
        {
            if (coreQuery.TryGetComponent(entity, out var coreComp))
            {
                _coreSystem.TryLinkComponent(entity, coreComp, uid, moderatorInput, (core, compUid) => core.ModeratorInputUid = compUid);
                break;
            }
        }
    }
}