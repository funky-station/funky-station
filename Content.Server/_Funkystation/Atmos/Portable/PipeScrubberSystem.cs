// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Shared.Atmos;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Funkystation.Atmos.Visuals;

namespace Content.Server._Funkystation.Atmos.Portable
{
    public sealed class PipeScrubberSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
        [Dependency] private readonly GasCanisterSystem _canister = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly AmbientSoundSystem _ambientSound = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PipeScrubberComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdate);
            SubscribeLocalEvent<PipeScrubberComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
            SubscribeLocalEvent<PipeScrubberComponent, ActivateInWorldEvent>(OnActivate);
            SubscribeLocalEvent<PipeScrubberComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<PipeScrubberComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<PipeScrubberComponent, GasAnalyzerScanEvent>(OnGasAnalyzerScan);
        }

        private bool IsFull(PipeScrubberComponent comp) => comp.Air.Pressure >= comp.MaxPressure;

        private void OnDeviceUpdate(EntityUid uid, PipeScrubberComponent comp, ref AtmosDeviceUpdateEvent args)
        {
            if (!_nodeContainer.TryGetNode(uid, comp.PortName, out PortablePipeNode? portNode) ||
                !portNode.ConnectionsEnabled)
            {
                UpdateAppearance(uid, comp, IsFull(comp), false, false, false);
                return;
            }

            var isFull = IsFull(comp);

            // Drain / merge when off (push to net)
            if (!comp.Enabled && portNode.NodeGroup is PipeNet { NodeCount: > 1 } net)
            {
                _canister.MixContainerWithPipeNet(comp.Air, net.Air);
            }

            // When on and full: disable connection to block net equalization / backflow
            if (comp.Enabled && isFull)
            {
                portNode.ConnectionsEnabled = false;
                UpdateAppearance(uid, comp, true, false, false, false);  // no draining visual
                return;
            }

            // Scrubbing / split when on and not full (pull from net)
            bool isRunning = false;

            if (comp.Enabled && portNode.NodeGroup is PipeNet sourceNet && sourceNet.NodeCount > 0)
            {
                var source = sourceNet.Air;
                if (source.TotalMoles > 0)
                {
                    var speedup = _atmosphere.PumpSpeedup();
                    var volumeSpace = comp.Air.Volume;
                    var pressureSpace = comp.MaxPressure - comp.Air.Pressure;

                    if (pressureSpace > 0)
                    {
                        var transferMoles = MathF.Min(
                            source.TotalMoles * (comp.TransferRate * speedup / source.Volume),
                            (pressureSpace * volumeSpace) / Atmospherics.R);

                        if (transferMoles > 0.001f)
                        {
                            var taken = source.Remove(transferMoles);
                            _atmosphere.Merge(comp.Air, taken);
                            isRunning = true;
                        }
                    }
                }
            }

            // Re-enable connection if we didn't disable it above
            portNode.ConnectionsEnabled = true;

            UpdateAppearance(uid, comp, isFull, comp.Enabled, isRunning, true);
        }

        private void OnAnchorStateChanged(EntityUid uid, PipeScrubberComponent comp, AnchorStateChangedEvent args)
        {
            UpdateAppearance(uid, comp, false, false, false, false);

            if (!_nodeContainer.TryGetNode(uid, comp.PortName, out PortablePipeNode? portNode))
                return;

            bool shouldConnect = args.Anchored && !(comp.Enabled && IsFull(comp));

            portNode.ConnectionsEnabled = shouldConnect;
        }

        private void OnActivate(EntityUid uid, PipeScrubberComponent comp, ActivateInWorldEvent args)
        {
            if (args.Handled)
                return;

            comp.Enabled = !comp.Enabled;

            var state = comp.Enabled ? "on" : "off";
            _popup.PopupEntity(Loc.GetString($"pipe-scrubber-turned-{state}"), uid, args.User);

            // Re-evaluate connection after toggle
            if (_nodeContainer.TryGetNode(uid, comp.PortName, out PortablePipeNode? portNode))
                portNode.ConnectionsEnabled = Transform(uid).Anchored && !(comp.Enabled && IsFull(comp));

            args.Handled = true;
        }

        private void OnExamined(EntityUid uid, PipeScrubberComponent comp, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            var percent = Math.Round((comp.Air.Pressure / comp.MaxPressure) * 100f);
            args.PushMarkup(Loc.GetString("pipe-scrubber-fill-level", ("percent", percent)));
        }

        private void OnDestroyed(EntityUid uid, PipeScrubberComponent comp, DestructionEventArgs args)
        {
            var env = _atmosphere.GetContainingMixture(uid, false, true);
            if (env != null)
                _atmosphere.Merge(env, comp.Air);

            _adminLogger.Add(LogType.CanisterPurged, LogImpact.Medium,
                $"Pipe scrubber {ToPrettyString(uid):entity} purged its contents of {comp.Air} into the environment.");

            comp.Air.Clear();
        }

        private void OnGasAnalyzerScan(EntityUid uid, PipeScrubberComponent comp, GasAnalyzerScanEvent args)
        {
            args.GasMixtures ??= new();
            args.GasMixtures.Add((Name(uid), comp.Air));
        }

        private void UpdateAppearance(EntityUid uid, PipeScrubberComponent comp, bool isFull, bool isEnabled, bool isRunning, bool isConnected)
        {
            _ambientSound.SetAmbience(uid, isRunning);

            _appearance.SetData(uid, PipeScrubberVisuals.IsFull, isFull);
            _appearance.SetData(uid, PipeScrubberVisuals.IsEnabled, isEnabled && isConnected);
            _appearance.SetData(uid, PipeScrubberVisuals.IsScrubbing, isRunning && isConnected);
            _appearance.SetData(uid, PipeScrubberVisuals.IsDraining, isConnected && !isEnabled);
        }
    }
}
