// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 aa5g21 <aa5g21@soton.ac.uk>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Silo.UI;

[UsedImplicitly]
public sealed class SiloRelayBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private SiloRelayMenu? _menu;

    public SiloRelayBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredRight<SiloRelayMenu>();
        _menu.SetEntity(Owner);
    }
}
