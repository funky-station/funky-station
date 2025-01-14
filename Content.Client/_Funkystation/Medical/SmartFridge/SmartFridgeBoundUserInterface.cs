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
        if (_cachedInventory.Count == 0)
            return;

        // creates a list of possible items to dispense, based on if the itemName matches
        IEnumerable<string> queryList =
            from item in _cachedInventory
            where item.ItemName == data.ItemName
            select item.StorageSlotId;

        var matchingItems = queryList
            .OrderByDescending(q => q)
            .ToList();
        var amountToEject = data.Amount;

        if (matchingItems.Count == 0)
            return;

        // trims the list depending on how much is needed to dispense
        var itemSlotsToEject = new List<string>();

        for (var i = 0; i < amountToEject.GetFixedPoint(); i++)
        {
            // i cant do this math in my head
            // this might need to be =< but idkz
            if (matchingItems.Count < i + 1)
                break;

            var addedEntry = matchingItems.ElementAtOrDefault(i);

            if (addedEntry == null)
                return;

            itemSlotsToEject.Add(addedEntry);
        }

        if (itemSlotsToEject.Count == 0)
            return;

        SendMessage(new SmartFridgeEjectMessage(itemSlotsToEject));
    }
}
