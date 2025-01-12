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

    [ViewVariables]
    private List<SmartFridgeInventoryItem> _cachedInventory = [];

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SmartFridgeMenu>();
        _menu.OpenCenteredLeft();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.OnItemSelected += OnItemSelected;

        Refresh();
    }

    public void Refresh()
    {
        var system = EntMan.System<SmartFridgeSystem>();
        _cachedInventory = system.GetInventoryClient(Owner);

        _menu?.Populate(_cachedInventory);
    }

    private void OnItemSelected(BaseButton.ButtonEventArgs args, SmartFridgeItem.DispenseButton data)
    {
        /*if (args.Function != EngineKeyFunctions.UIClick)
            return;*/

        /*if (data is not FridgeItemsListData { ItemIndex: var itemIndex })
            return;*/
        // probably important checks to keep but idrk how i could translate them rn sozzzzz

        if (_cachedInventory.Count == 0)
            return;

        var matchingItems = new List<string>();

        // creates a list of possible items to dispense
        foreach (var item in _cachedInventory)
        {
            if (data.ItemName == item.ItemName)
                matchingItems.Add(item.StorageSlotId);
        }

        if (matchingItems.Count == 0)
            return;

        var amountToEject = data.Amount.GetFixedPoint();
        var itemSlotsToEject = new List<string>();

        for (int i = 0; i < amountToEject; i++)
        {
            if (matchingItems.Count > i)
                return;

            itemSlotsToEject.Add(matchingItems[i]);
        }

        SendMessage(new SmartFridgeEjectMessage(itemSlotsToEject));
    }
}
