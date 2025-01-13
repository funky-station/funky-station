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

        // creates a list of possible items to dispense, based on if the itemName matches
        // entProto doesn't work for matching the items sooooo.
        var matchingItems = new List<string>();

        // theyre telling me this could be a linq query but i dont know how to do that so i dont care
        foreach (var item in _cachedInventory)
        {
            if (data.ItemName == item.ItemName)
                matchingItems.Add(item.StorageSlotId);
        }

        if (matchingItems.Count == 0)
            return;

        var amountToEject = data.Amount;
        var itemSlotsToEject = new List<string>();

        for (var i = 0; i < amountToEject.GetFixedPoint(); i++)
        {
            if (matchingItems.Count < i + 1)
                break;

            var addedEntry = matchingItems.ElementAtOrDefault(i);

            if (addedEntry == null)
                return;

            itemSlotsToEject.Add(addedEntry);
        }

        if (itemSlotsToEject.Count == 0)
            return;

        // remove data.Amount sending ltr
        // it doesnt need that info anymore
        SendMessage(new SmartFridgeEjectMessage(itemSlotsToEject, data.Amount));
    }
}
