// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._Funkystation.Cargo.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared._Funkystation.Atmos.EntitySystems;
using Content.Shared._Funkystation.Cargo.Components;
using Content.Shared._Funkystation.CCVars;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.Cargo.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Atmos.EntitySystems;

[UsedImplicitly]
public sealed class GasExtractorSystem : SharedGasExtractorSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GasExtractorConsoleSystem _gasExtractorConsole = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasExtractorComponent, AtmosDeviceUpdateEvent>(OnExtractorUpdated);
        SubscribeLocalEvent<GasExtractorComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnExtractorUpdated(Entity<GasExtractorComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var extractor = ent.Comp;
        var oldState = extractor.ExtractorState;

        if (!TryComp<DeviceLinkSinkComponent>(ent, out var sink) || sink.LinkedSources.Count == 0)
        {
            extractor.ExtractorState = GasExtractorState.Disabled;

            if (extractor.ExtractorState != oldState)
                Dirty(ent);

            return;
        }

        float toSpawn;

        if (!GetValidEnvironment(ent, out var environment) || !Transform(ent).Anchored)
        {
            extractor.ExtractorState = GasExtractorState.Disabled;
        }
        else if ((toSpawn = CapSpawnAmount(ent, extractor.SpawnAmount * args.dt, environment)) <= 0f)
        {
            extractor.ExtractorState = GasExtractorState.Idle;
        }
        else
        {
            extractor.ExtractorState = GasExtractorState.Working;

            float originalToSpawn = toSpawn;
            float molesToSpawn = originalToSpawn;

            if (_cfg.GetCVar(CCVars_Funky.GasExtractorsRequirePayment))
            {
                // Only apply priced extractor logic if at least one linked console has positive multiplier
                bool hasPricedConsole = sink.LinkedSources.Any(src =>
                    TryComp<GasExtractorConsoleComponent>(src, out var console) &&
                    console.PriceMultiplier > 0f);

                if (hasPricedConsole &&
                    _proto.TryIndex<GasPrototype>(((int)extractor.SpawnGas).ToString(), out var gasProto) &&
                    gasProto.PricePerMole > 0f)
                {
                    // Attempt auto-buy if enabled and insufficient moles
                    if (extractor.AutoBuyEnabled && extractor.RemainingMoles < originalToSpawn)
                    {
                        float deficit = originalToSpawn - extractor.RemainingMoles;
                        if (TryGetFirstValidConsole(ent, out var consoleUid, out var orderConsole))
                        {
                            float buyMoles = deficit * 1.05f;
                            _gasExtractorConsole.TryAutoPurchaseMoles(consoleUid, orderConsole, ent, buyMoles);
                        }
                    }

                    // Deduct from remaining budget
                    float canAfford = Math.Min(originalToSpawn, extractor.RemainingMoles);

                    if (canAfford < Atmospherics.GasMinMoles)
                    {
                        extractor.ExtractorState = GasExtractorState.Idle;
                        molesToSpawn = 0f;
                    }
                    else
                    {
                        extractor.RemainingMoles -= canAfford;
                        molesToSpawn = canAfford;
                        Dirty(ent);
                    }
                }
            }

            // Spawn the gas (either full amount or what was afforded)
            var merger = new GasMixture(1) { Temperature = extractor.SpawnTemperature };
            merger.SetMoles(extractor.SpawnGas, molesToSpawn);
            _atmosphereSystem.Merge(environment, merger);
        }

        if (extractor.ExtractorState != oldState)
        {
            Dirty(ent);
        }
    }

    private bool TryGetFirstValidConsole(
        Entity<GasExtractorComponent> extractorEnt,
        out EntityUid consoleUid,
        out CargoOrderConsoleComponent orderConsole)
    {
        consoleUid = default;
        orderConsole = default!;

        if (!TryComp<DeviceLinkSinkComponent>(extractorEnt, out var sink) || sink.LinkedSources.Count == 0)
            return false;

        foreach (var sourceUid in sink.LinkedSources)
        {
            if (!TryComp<GasExtractorConsoleComponent>(sourceUid, out var console))
                continue;

            if (console.PriceMultiplier <= 0f)
                continue;

            if (!TryComp<CargoOrderConsoleComponent>(sourceUid, out var oc))
                continue;

            consoleUid = sourceUid;
            orderConsole = oc;
            return true;
        }

        return false;
    }

    private void OnPortDisconnected(Entity<GasExtractorComponent> ent, ref PortDisconnectedEvent args)
    {
        ent.Comp.SpawnAmount = 0f;
        ent.Comp.MaxExternalPressure = 0f;
        ent.Comp.ExtractorState = GasExtractorState.Disabled;

        Dirty(ent);

        if (TryComp<DeviceLinkSinkComponent>(ent, out var sink))
        {
            foreach (var consoleUid in sink.LinkedSources.ToList())
            {
                if (!TryComp<GasExtractorConsoleComponent>(consoleUid, out var console))
                    continue;

                if (console.LinkedExtractors.Remove(ent))
                    Dirty(consoleUid, console);
            }
        }
    }

    private bool GetValidEnvironment(Entity<GasExtractorComponent> ent, [NotNullWhen(true)] out GasMixture? environment)
    {
        var (uid, extractor) = ent;
        var transform = Transform(uid);
        var position = _transformSystem.GetGridOrMapTilePosition(uid, transform);

        // Treat space as an invalid environment
        if (_atmosphereSystem.IsTileSpace(transform.GridUid, transform.MapUid, position))
        {
            environment = null;
            return false;
        }

        environment = _atmosphereSystem.GetContainingMixture((uid, transform), true, true);
        return environment != null;
    }

    private float CapSpawnAmount(Entity<GasExtractorComponent> ent, float toSpawnTarget, GasMixture environment)
    {
        var (uid, extractor) = ent;

        // How many moles could we theoretically spawn. Cap by pressure and amount.
        var allowableMoles = Math.Min(
            (extractor.MaxExternalPressure - environment.Pressure) * environment.Volume / (extractor.SpawnTemperature * Atmospherics.R),
            extractor.MaxExternalAmount - environment.TotalMoles);

        var toSpawnReal = Math.Clamp(allowableMoles, 0f, toSpawnTarget);

        if (toSpawnReal < Atmospherics.GasMinMoles) {
            return 0f;
        }

        return toSpawnReal;
    }
}
