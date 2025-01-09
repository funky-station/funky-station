using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;
using Content.Client._Funkystation.Medical.SmartFridge.UI;
using Content.Shared._Funkystation.Medical.SmartFridge;
using OpenToolkit.GraphicsLibraryFramework;
using Robust.Client.UserInterface.Controls;
using SmartFridgeMenu = Content.Client._Funkystation.Medical.SmartFridge.UI.SmartFridgeMenu;

namespace Content.Client._Funkystation.Medical.SmartFridge;

public sealed class SmartFridgeBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private SmartFridgeMenu? _menu;
    private SmartFridgeItem? _items;

    [ViewVariables]
    private List<SmartFridgeInventoryItem> _cachedInventory = [];

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SmartFridgeMenu>();
        _menu.OpenCenteredLeft();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        //_menu.OnItemSelected += OnItemSelected;

        if (_items != null) // lol? idk what else do i do
        {
            _items.OnItemSelected += OnItemSelected;
        }

        Refresh();
    }

    public void Refresh()
    {
        var system = EntMan.System<SmartFridgeSystem>();
        _cachedInventory = system.GetInventoryClient(Owner);

        _menu?.Populate(_cachedInventory);
    }

    // listcontainerbutton dispense event
    private void OnItemSelected(BaseButton.ButtonEventArgs args, SmartFridgeItem.DispenseButton data)
    {
        /*if (args.Function != EngineKeyFunctions.UIClick)
            return;*/

        /*if (data is not FridgeItemsListData { ItemIndex: var itemIndex })
            return;*/
        // probably important checks to keep but idrk how i could translate them rn sozzzzz

        if (_cachedInventory.Count == 0)
            return;

        var selectedItem = _cachedInventory.ElementAtOrDefault(data.Index);

        if (selectedItem == null)
            return;

        SendMessage(new SmartFridgeEjectMessage(selectedItem.StorageSlotId, data.Amount));
    }
}
