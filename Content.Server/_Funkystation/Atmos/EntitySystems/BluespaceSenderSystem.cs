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
/// Handles server-side logic for Bluespace Senders.
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
        SubscribeLocalEvent<BluespaceSenderComponent, BluespaceSenderChangeEnabledGasesMessage>(OnChangeEnabledGases);
        SubscribeLocalEvent<BluespaceSenderComponent, BluespaceSenderChangeRetrieveMessage>(OnChangeRetrieve);
        SubscribeLocalEvent<BluespaceSenderComponent, AtmosDeviceUpdateEvent>(OnDeviceAtmosUpdate);
    }

    private void OnStartup(EntityUid uid, BluespaceSenderComponent sender, ComponentStartup args)
    {
        // Enable Oxygen and Nitrogen by default
        sender.BluespaceSenderEnabledList[0] = true;
        sender.BluespaceSenderEnabledList[1] = true;
        DirtyUI(uid, sender);
    }

    private void OnChangeEnabledGases(EntityUid uid, BluespaceSenderComponent sender, BluespaceSenderChangeEnabledGasesMessage args)
    {
        sender.BluespaceSenderEnabledList[args.Index] = !sender.BluespaceSenderEnabledList[args.Index];
        DirtyUI(uid, sender);
    }

    private void OnChangeRetrieve(EntityUid uid, BluespaceSenderComponent sender, BluespaceSenderChangeRetrieveMessage args)
    {
        sender.BluespaceSenderRetrieveList[args.Index] = !sender.BluespaceSenderRetrieveList[args.Index];
        DirtyUI(uid, sender);
    }

    private void OnDeviceAtmosUpdate(EntityUid uid, BluespaceSenderComponent sender, ref AtmosDeviceUpdateEvent args)
    {
        if (!_power.IsPowered(uid) || !TryComp<ApcPowerReceiverComponent>(uid, out _))
            return;

        var retrieveList = sender.BluespaceSenderRetrieveList;
        var retrieveCount = retrieveList.Count(x => x);
        var bluespaceMixture = sender.BluespaceGasMixture;

        if (!GetBluespaceSenderPipeMixture(uid, sender, out var pipeGasMixture) || pipeGasMixture == null)
            return;

        if (retrieveCount == 0)
        {
            // When not retrieving, move all pipe gas to bluespace storage
            var removedGases = pipeGasMixture.RemoveRatio(1f);
            removedGases.Temperature = Atmospherics.T20C;
            _atmos.Merge(bluespaceMixture, removedGases);
        }
        else
        {
            // When retrieving, add selected gases from bluespace to pipe
            if (pipeGasMixture.Pressure >= 4500)
                return;

            for (var i = 0; i < retrieveList.Count; i++)
            {
                if (!retrieveList[i])
                    continue;

                var moles = ((4500 - pipeGasMixture.Pressure) * 200) / (Atmospherics.R * Atmospherics.T20C) / retrieveCount;
                moles = Math.Min(moles, bluespaceMixture.GetMoles(i));
                
                bluespaceMixture.AdjustMoles(i, -moles);
                var addedGases = new GasMixture();
                addedGases.AdjustMoles(i, moles);
                addedGases.Temperature = Atmospherics.T20C;
                _atmos.Merge(pipeGasMixture, addedGases);
            }
        }

        UpdateOnCountdown(uid, sender);
    }

    private void DirtyUI(EntityUid uid, BluespaceSenderComponent? sender = null)
    {
        if (!Resolve(uid, ref sender))
            return;

        _ui.SetUiState(uid, BluespaceSenderUiKey.Key,
            new BluespaceSenderBoundUserInterfaceState(
                Name(uid),
                sender.BluespaceGasMixture,
                sender.BluespaceSenderEnabledList,
                sender.BluespaceSenderRetrieveList));
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