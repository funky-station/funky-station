using Content.Shared._Funkystation.Atmos.Piping.Binary.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Atmos.UI
{
    [UsedImplicitly]
    public sealed class TemperatureGateBoundUserInterface : BoundUserInterface
    {
        private TemperatureGateWindow? _window;

        public TemperatureGateBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<TemperatureGateWindow>();

            _window.OnStatusToggled += () =>
                SendMessage(new TemperatureGateToggleEnabledMessage(_window.Enabled));

            _window.OnThresholdAndModeSet += (threshold, isMin) =>
                SendMessage(new TemperatureGateSetThresholdAndModeMessage(threshold, isMin));
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (_window == null || state is not TemperatureGateBoundUserInterfaceState cast)
                return;

            _window.Title = cast.DeviceName;
            _window.SetStatus(cast.Enabled);
            _window.SetThreshold(cast.Threshold);
            _window.SetMode(cast.IsMinMode);
        }
    }
}
