using Content.Server._Funkystation.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRWasteOutputSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HFRCoreSystem _coreSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly HypertorusFusionReactorSystem _hfrSystem = default!;
    [Dependency] private readonly HFRSidePartSystem _hfrSidePartSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HFRWasteOutputComponent, AnchorStateChangedEvent>(OnAnchorChanged);
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
            _hfrSidePartSystem.TryFindCore(uid);
        }
    }
}