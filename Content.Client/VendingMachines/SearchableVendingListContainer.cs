// SPDX-FileCopyrightText: 2025 Tojo <32783144+Alecksohs@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;

namespace Content.Client.VendingMachines;

public sealed class SearchableVendingListContainer : SearchListContainer
{
    public override IListEntry InitializeControl(ListData data, int index)
    {
        return new VendingMachineEntry(data, index);
    }
}
