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
    /// Initializes a <see cref="BluespaceSenderWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class BluespaceSenderBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private BluespaceSenderWindow? _window;

        public BluespaceSenderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
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
            _window = this.CreateWindow<BluespaceSenderWindow>();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

            // Setup static button actions.
            _window.RetrieveButtonPressed += RetrieveButtonPressed;
            _window.ToggleStatusButtonPressed += OnToggleStatusButtonPressed;
            _window.RetrieveModeButtonPressed += RetrieveModeButtonPressed;
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not BluespaceSenderBoundUserInterfaceState cast)
                return;
                
            _window.BuildGasList(cast.BluespaceSenderRetrieveList, cast.BluespaceGasMixture);
            _window.SetActive(cast.PowerToggle);
            _window.SetRetrievingMode(cast.InRetrieveMode);
        }

        private void RetrieveButtonPressed(int index)
        {
            SendMessage(new BluespaceSenderChangeRetrieveMessage(index));
        }

        private void OnToggleStatusButtonPressed()
        {
            if (_window is null) 
                return;

            SendMessage(new BluespaceSenderToggleMessage());
        }

        private void RetrieveModeButtonPressed()
        {
            SendMessage(new BluespaceSenderToggleRetrieveModeMessage());
        }
    }
}
