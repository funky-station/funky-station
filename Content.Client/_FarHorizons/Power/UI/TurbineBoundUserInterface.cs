// SPDX-FileCopyrightText: 2025 jhrushbe <capnmerry@gmail.com>
// SPDX-FileCopyrightText: 2025 rottenheadphones <juaelwe@outlook.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: CC-BY-NC-SA-3.0

using Content.Client.UserInterface;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using JetBrains.Annotations;
using Robust.Client.Timing;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="TurbineWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class TurbineBoundUserInterface : BoundUserInterface, IBuiPreTickUpdate
{
    [Dependency] private readonly IClientGameTiming _gameTiming = null!;

    [ViewVariables]
    private TurbineWindow? _window;

    private BuiPredictionState? _pred;
    private InputCoalescer<float> _flowRateCoalescer;
    private InputCoalescer<float> _statorLoadCoalescer;

    public TurbineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _pred = new BuiPredictionState(this, _gameTiming);

        _window = this.CreateWindow<TurbineWindow>();
        _window.SetEntity(Owner);

        _window.TurbineFlowRateChanged += val => _flowRateCoalescer.Set(val);
        _window.TurbineStatorLoadChanged += val => _statorLoadCoalescer.Set(val);
        Update();
    }

    void IBuiPreTickUpdate.PreTickUpdate()
    {
        if (_flowRateCoalescer.CheckIsModified(out var flowRateValue))
            _pred!.SendMessage(new TurbineChangeFlowRateMessage(flowRateValue));

        if (_statorLoadCoalescer.CheckIsModified(out var statorLoadValue))
            _pred!.SendMessage(new TurbineChangeStatorLoadMessage(statorLoadValue));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not TurbineBuiState turbineState)
            return;

        foreach (var replayMsg in _pred!.MessagesToReplay())
        {
            switch (replayMsg)
            {
                case TurbineChangeFlowRateMessage setFlowRate:
                    turbineState.FlowRate = setFlowRate.FlowRate;
                    break;

                case TurbineChangeStatorLoadMessage setStatorLoad:
                    turbineState.StatorLoad = Math.Clamp(setStatorLoad.StatorLoad, 1000f, 500000f); // The nasty hard-coded gremlin
                    break;
            }
        }

        _window?.Update(turbineState);
    }
}