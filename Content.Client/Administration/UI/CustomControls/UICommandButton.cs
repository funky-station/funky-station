// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leo <lzimann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;

namespace Content.Client.Administration.UI.CustomControls
{
    public sealed class UICommandButton : CommandButton
    {
        public Type? WindowType { get; set; }
        private DefaultWindow? _window;

        protected override void Execute(ButtonEventArgs obj)
        {
            if (WindowType == null)
                return;
            _window = (DefaultWindow) IoCManager.Resolve<IDynamicTypeFactory>().CreateInstance(WindowType);
            _window?.OpenCentered();
        }
    }
}
