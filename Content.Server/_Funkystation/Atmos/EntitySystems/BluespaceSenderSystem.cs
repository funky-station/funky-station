// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using System.Linq;
using Content.Server._Funkystation.Atmos.Components;

namespace Content.Server._Funkystation.Atmos.EntitySystems;

/// <summary>
/// Handles server-side logic for bluespace senders.
/// </summary>
public sealed class BluespaceSenderSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private TimeSpan _lastUpdateTime = TimeSpan.Zero;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceSenderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BluespaceSenderComponent, BluespaceSenderChangeRetrieveMessage>(OnChangeRetrieve);
        SubscribeLocalEvent<BluespaceSenderComponent, BluespaceSenderToggleMessage>(OnToggleStatus);
        SubscribeLocalEvent<BluespaceSenderComponent, BluespaceSenderToggleRetrieveModeMessage>(OnToggleRetrieveMode);
        SubscribeLocalEvent<BluespaceSenderComponent, AtmosDeviceUpdateEvent>(OnDeviceAtmosUpdate);
    }

    private void OnStartup(EntityUid uid, BluespaceSenderComponent sender, ComponentStartup args)
    {
        DirtyUI(uid, sender);
    }

    private void OnChangeRetrieve(EntityUid uid, BluespaceSenderComponent sender, BluespaceSenderChangeRetrieveMessage args)
    {
        sender.BluespaceSenderRetrieveList[args.Index] = !sender.BluespaceSenderRetrieveList[args.Index];
        DirtyUI(uid, sender);
    }

    private void OnDeviceAtmosUpdate(EntityUid uid, BluespaceSenderComponent sender, ref AtmosDeviceUpdateEvent args)
    {
        if (!_power.IsPowered(uid) || !TryComp<ApcPowerReceiverComponent>(uid, out _)
            || !GetBluespaceSenderPipeMixture(uid, sender, out var pipeGasMixture) || pipeGasMixture == null)
            return;

        var inRetrieveMode = sender.InRetrieveMode;
        var bluespaceMixture = sender.BluespaceGasMixture;

        if (inRetrieveMode)
        {
            var retrieveList = sender.BluespaceSenderRetrieveList;
            int retrieveCount = 0;

            for (int i = 0; i < retrieveList.Count; i++)
            {
                if (retrieveList[i] && bluespaceMixture.GetMoles(i) > 0.01f)
                    retrieveCount++;
            }

            if (retrieveCount < 1 || pipeGasMixture.Pressure >= 4500)
            {
                UpdateOnCountdown(uid, sender);
                return;
            }

            var initMoles = ((4500 - pipeGasMixture.Pressure) * 200) / (Atmospherics.R * Atmospherics.T20C) / retrieveCount;
            for (var i = 0; i < retrieveList.Count; i++)
            {
                if (!retrieveList[i])
                    continue;

                var moles = initMoles;
                moles = Math.Min(moles, bluespaceMixture.GetMoles(i));
                
                bluespaceMixture.AdjustMoles(i, -moles);
                var addedGases = new GasMixture();
                addedGases.AdjustMoles(i, moles);
                addedGases.Temperature = Atmospherics.T20C;
                _atmos.Merge(pipeGasMixture, addedGases);
            }
        }
        else
        {
            // When not retrieving, move all pipe gas to bluespace storage
            var removedGases = pipeGasMixture.RemoveRatio(1f);
            removedGases.Temperature = Atmospherics.T20C;
            _atmos.Merge(bluespaceMixture, removedGases);
        }

        UpdateOnCountdown(uid, sender);
    }

    private void OnToggleStatus(EntityUid uid, BluespaceSenderComponent sender, BluespaceSenderToggleMessage args)
    {
        _power.TogglePower(uid);
        sender.PowerToggle = _power.IsPowered(uid);
        DirtyUI(uid, sender);
    }

    private void OnToggleRetrieveMode(EntityUid uid, BluespaceSenderComponent sender, BluespaceSenderToggleRetrieveModeMessage args)
    {
        sender.InRetrieveMode = !sender.InRetrieveMode;
        DirtyUI(uid, sender);
    }

    private void DirtyUI(EntityUid uid, BluespaceSenderComponent? sender = null)
    {
        if (!Resolve(uid, ref sender))
            return;

        _ui.SetUiState(uid, BluespaceSenderUiKey.Key,
            new BluespaceSenderBoundUserInterfaceState(
                Name(uid),
                sender.BluespaceGasMixture,
                sender.BluespaceSenderRetrieveList,
                sender.PowerToggle,
                sender.InRetrieveMode));
    }

    private bool GetBluespaceSenderPipeMixture(EntityUid uid, BluespaceSenderComponent sender, out GasMixture? mixture)
    {
        mixture = null;
        if (!_nodeContainer.TryGetNode(uid, sender.InletName, out PipeNode? inlet))
            return false;
        
        mixture = inlet.Air;
        return true;
    }

    private void UpdateOnCountdown(EntityUid uid, BluespaceSenderComponent sender)
    {
        if (_gameTiming.CurTime < _lastUpdateTime + _updateInterval)
            return;

        DirtyUI(uid, sender);
        _lastUpdateTime = _gameTiming.CurTime;
    }
}