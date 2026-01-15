// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._Funkystation.Cargo.Systems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared._Funkystation.Cargo.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.Cargo.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.Atmos.EntitySystems;

[UsedImplicitly]
public sealed class GasMinerSystem : SharedGasMinerSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!; // Funkystation
    [Dependency] private readonly GasMinerConsoleSystem _gasMinerConsole = default!; // Funkystation
    [Dependency] private readonly StationSystem _station = default!; // Funkystation
    [Dependency] private readonly CargoSystem _cargo = default!; // Funkystation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasMinerComponent, AtmosDeviceUpdateEvent>(OnMinerUpdated);
        SubscribeLocalEvent<GasMinerComponent, PortDisconnectedEvent>(OnPortDisconnected); // Funkystation
    }

    private void OnMinerUpdated(Entity<GasMinerComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var miner = ent.Comp;
        var oldState = miner.MinerState;
        float toSpawn;

        if (!GetValidEnvironment(ent, out var environment) || !Transform(ent).Anchored)
        {
            miner.MinerState = GasMinerState.Disabled;
        }
        // SpawnAmount is declared in mol/s so to get the amount of gas we hope to mine, we have to multiply this by
        // how long we have been waiting to spawn it and further cap the number according to the miner's state.
        else if ((toSpawn = CapSpawnAmount(ent, miner.SpawnAmount * args.dt, environment)) == 0)
        {
            miner.MinerState = GasMinerState.Idle;
        }
        else
        {
            miner.MinerState = GasMinerState.Working;

            // Funkystation - Deduct gas funds from linked consoles.
            // If not an extractor, miners work as normal.
            float originalToSpawn = toSpawn;

            // Budget and auto-buy for linked extractors
            if (TryComp<DeviceLinkSinkComponent>(ent, out var sink) && sink.LinkedSources.Count > 0)
            {
                // Auto-buy attempt if enabled and deficit
                if (miner.AutoBuyEnabled && originalToSpawn > 0)
                {
                    float deficit = originalToSpawn - miner.RemainingMoles;

                    if (deficit > 0)
                    {
                        var consoleUid = sink.LinkedSources.First();

                        if (TryComp<GasMinerConsoleComponent>(consoleUid, out var console) &&
                            TryComp<CargoOrderConsoleComponent>(consoleUid, out var orderConsole))
                        {
                            float buyMoles = deficit * 1.05f;

                            _gasMinerConsole.TryAutoPurchaseMoles(
                                consoleUid,
                                orderConsole,
                                ent,
                                buyMoles);
                        }
                    }
                }

                // Deduct from budget
                float canAfford = Math.Min(originalToSpawn, miner.RemainingMoles);

                if (canAfford < Atmospherics.GasMinMoles)
                {
                    miner.MinerState = GasMinerState.Idle;
                    toSpawn = 0f;
                }
                else
                {
                    miner.RemainingMoles -= canAfford;
                    toSpawn = canAfford;
                    Dirty(ent);
                }
            }
            // End of Funkystation changes

            // Time to mine some gas.
            var merger = new GasMixture(1) { Temperature = miner.SpawnTemperature };
            merger.SetMoles(miner.SpawnGas, toSpawn);
            _atmosphereSystem.Merge(environment, merger);
        }

        if (miner.MinerState != oldState)
        {
            Dirty(ent);
        }
    }

    // Funkstation - disable disconnected miners
    private void OnPortDisconnected(Entity<GasMinerComponent> ent, ref PortDisconnectedEvent args)
    {
        ent.Comp.SpawnAmount = 0f;
        ent.Comp.MaxExternalPressure = 0f;
        ent.Comp.MinerState = GasMinerState.Disabled;

        Dirty(ent);

        if (TryComp<DeviceLinkSinkComponent>(ent, out var sink))
        {
            foreach (var consoleUid in sink.LinkedSources.ToList())
            {
                if (!TryComp<GasMinerConsoleComponent>(consoleUid, out var console))
                    continue;

                if (console.LinkedMiners.Remove(ent))
                    Dirty(consoleUid, console);
            }
        }
    }
    // End of Funkystation changes

    private bool GetValidEnvironment(Entity<GasMinerComponent> ent, [NotNullWhen(true)] out GasMixture? environment)
    {
        var (uid, miner) = ent;
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

    private float CapSpawnAmount(Entity<GasMinerComponent> ent, float toSpawnTarget, GasMixture environment)
    {
        var (uid, miner) = ent;

        // How many moles could we theoretically spawn. Cap by pressure and amount.
        var allowableMoles = Math.Min(
            (miner.MaxExternalPressure - environment.Pressure) * environment.Volume / (miner.SpawnTemperature * Atmospherics.R),
            miner.MaxExternalAmount - environment.TotalMoles);

        var toSpawnReal = Math.Clamp(allowableMoles, 0f, toSpawnTarget);

        if (toSpawnReal < Atmospherics.GasMinMoles) {
            return 0f;
        }

        return toSpawnReal;
    }
}
