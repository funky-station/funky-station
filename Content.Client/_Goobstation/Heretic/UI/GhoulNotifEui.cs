// SPDX-FileCopyrightText: 2025 Kandiyaki <106633914+Kandiyaki@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;

namespace Content.Client._Goobstation.Heretic.UI;

public sealed class GhoulNotifEui : BaseEui
{
    private readonly GhoulNotifMenu _menu;

    public GhoulNotifEui()
    {
        _menu = new GhoulNotifMenu();
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
