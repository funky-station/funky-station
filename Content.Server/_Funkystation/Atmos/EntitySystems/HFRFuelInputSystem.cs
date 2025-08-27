// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Funkystation.Atmos.Components;
using Robust.Shared.Map.Components;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRFuelInputSystem : EntitySystem
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
        SubscribeLocalEvent<HFRFuelInputComponent, AnchorStateChangedEvent>(OnAnchorChanged);
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
            _hfrSidePartSystem.TryFindCore(uid);
        }
    }
}