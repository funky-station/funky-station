// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using static Content.Shared.Atmos.Components.GasAnalyzerComponent;

namespace Content.Client.Atmos.UI
{
    public sealed class GasAnalyzerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private GasAnalyzerWindow? _window;

        public GasAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindowCenteredLeft<GasAnalyzerWindow>();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;
            if (message is not GasAnalyzerUserMessage cast)
                return;
            _window.Populate(cast);
        }

        /// <summary>
        /// Closes UI and tells the server to disable the analyzer
        /// </summary>
        private void OnClose()
        {
            SendMessage(new GasAnalyzerDisableMessage());
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _window?.Dispose();
        }
    }
}
