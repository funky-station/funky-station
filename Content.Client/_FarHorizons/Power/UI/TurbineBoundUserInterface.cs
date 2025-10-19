using Content.Client.UserInterface;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.IdentityManagement;
using Content.Shared.Localizations;
using Content.Shared.Power;
using JetBrains.Annotations;
using Robust.Client.Timing;
using Robust.Client.UserInterface;
using static Robust.Client.UserInterface.Controls.MenuBar;

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

    //public override void Update()
    //{
    //    base.Update();

    //    if (_window is null || !EntMan.TryGetComponent(Owner, out TurbineComponent? turbine))
    //        return;

    //    _window.Title = Identity.Name(Owner, EntMan);
    //    _window.SetFlowRate(turbine.FlowRate);
    //    _window.SetStatorLoad(turbine.StatorLoad);
    //}

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
                    turbineState.StatorLoad = setStatorLoad.StatorLoad;
                    break;
            }
        }

        _window?.Update(turbineState);
    }
}