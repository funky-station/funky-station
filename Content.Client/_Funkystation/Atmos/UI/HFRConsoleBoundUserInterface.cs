// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Funkystation.Atmos.Components;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Atmos.UI;

public sealed class HFRConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private HFRConsoleWindow? _menu;

    public HFRConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<HFRConsoleWindow>();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.OpenCentered();
        _menu.OnClose += Close;

        // Toggle buttons
        _menu.TogglePowerButton.OnPressed += _ =>
        {
            _menu.SetActive(!_menu.Active);
            SendMessage(new HFRConsoleTogglePowerMessage());
        };
        _menu.ToggleCoolingButton.OnPressed += _ =>
        {
            _menu.SetCooling(!_menu.Cooling);
            SendMessage(new HFRConsoleToggleCoolingMessage());
        };
        _menu.ToggleFuelInjectionButton.OnPressed += _ =>
        {
            _menu.SetFuelInjecting(!_menu.FuelInjecting);
            SendMessage(new HFRConsoleToggleFuelInjectionMessage());
        };
        _menu.ToggleModeratorInjectionButton.OnPressed += _ =>
        {
            _menu.SetModeratorInjecting(!_menu.ModeratorInjecting);
            SendMessage(new HFRConsoleToggleModeratorInjectionMessage());
        };
        _menu.ToggleWasteRemoveButton.OnPressed += _ =>
        {
            _menu.SetWasteRemoving(!_menu.WasteRemoving);
            SendMessage(new HFRConsoleToggleWasteRemoveMessage());
        };

        // Recipe selection
        _menu.OnSelectRecipe += recipeId => SendMessage(new HFRConsoleSelectRecipeMessage(recipeId));

        // Inputs
        _menu.OnSetFuelInputRate += rate => SendMessage(new HFRConsoleSetFuelInputRateMessage(rate));
        _menu.OnSetModeratorInputRate += rate => SendMessage(new HFRConsoleSetModeratorInputRateMessage(rate));
        _menu.OnSetHeatingConductor += rate => SendMessage(new HFRConsoleSetHeatingConductorMessage(rate));
        _menu.OnSetCoolingVolume += rate => SendMessage(new HFRConsoleSetCoolingVolumeMessage(rate));
        _menu.OnSetMagneticConstrictor += rate => SendMessage(new HFRConsoleSetMagneticConstrictorMessage(rate));
        _menu.OnSetCurrentDamper += rate => SendMessage(new HFRConsoleSetCurrentDamperMessage(rate));
        _menu.OnSetModeratorFilteringRate += rate => SendMessage(new HFRConsoleSetModeratorFilteringRateMessage(rate));

        // Gas filter
        _menu.OnSetFilterGases += gases => SendMessage(new HFRConsoleSetFilterGasesMessage(gases));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not HFRConsoleBoundInterfaceState castState)
            return;

        _menu?.UpdateState(castState);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_menu == null)
            return;

        switch (message)
        {
            case HFRConsoleUpdateReactorMessage reactorMessage:
                _menu.SetReactorState(reactorMessage);
                break;
        }
    }
}