// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared._Funkystation.Atmos.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Atmos.UI
{
    /// <summary>
    /// Initializes a <see cref="BluespaceVendorWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class BluespaceVendorBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private BluespaceVendorWindow? _window;

        public BluespaceVendorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        /// <summary>
        /// Called each time a chem master UI instance is opened. Generates the window and fills it with
        /// relevant info. Sets the actions for static buttons.
        /// </summary>
        protected override void Open()
        {
            base.Open();

            // Setup window layout/elements
            _window = this.CreateWindow<BluespaceVendorWindow>();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

            // Setup static button actions.
            _window.RetrieveButtonPressed += OnRetrieveButtonPressed;
            _window.TankFillButtonPressed += (index) => SendMessage(new BluespaceVendorFillTankMessage(index));
            _window.TankEjectButtonPressed += OnTankEjectPressed;
            _window.TankEmptyButtonPressed += OnTankEmptyPressed;
            _window.ReleasePressureSet += OnReleasePressureSet;
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not BluespaceVendorBoundUserInterfaceState cast)
                return;

            _window.ToggleEjectButton(cast.TankLabel, cast.TankGasMixture);
            _window.ToggleEmptyTankButton(cast.TankLabel, cast.BluespaceSenderConnected);
            _window.ToggleGasList(cast.BluespaceSenderConnected, cast.BluespaceGasMixture, cast.BluespaceVendorRetrieveList, cast.TankGasMixture);
            _window.SetReleasePressureSpinbox(cast.ReleasePressure);
        }

        private void OnRetrieveButtonPressed(int index)
        {
            SendMessage(new BluespaceVendorChangeRetrieveMessage(index));
        }

        private void OnTankFillPressed(int index)
        {
            SendMessage(new BluespaceVendorFillTankMessage(index));
        }

        private void OnTankEjectPressed()
        {
            SendMessage(new BluespaceVendorHoldingTankEjectMessage());
        }

        private void OnTankEmptyPressed()
        {
            SendMessage(new BluespaceVendorHoldingTankEmptyMessage());
        }

        private void OnReleasePressureSet(float value)
        {
            SendMessage(new BluespaceVendorChangeReleasePressureMessage(value));
        }
    }
}
