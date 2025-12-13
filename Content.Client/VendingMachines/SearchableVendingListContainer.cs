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
