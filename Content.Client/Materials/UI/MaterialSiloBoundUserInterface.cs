// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Materials.MaterialSilo;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Materials.UI;

[UsedImplicitly]
public sealed class MaterialSiloBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MaterialSiloMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MaterialSiloMenu>();
        _menu.SetEntity(Owner);

        _menu.OnClientEntryPressed += netEnt =>
        {
            SendPredictedMessage(new ToggleMaterialSiloClientMessage(netEnt));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MaterialSiloBuiState msg)
            return;
        _menu?.Update(msg);
    }
}
