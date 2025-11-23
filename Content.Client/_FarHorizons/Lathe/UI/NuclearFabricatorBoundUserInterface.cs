using Content.Client.Lathe.UI;
using Content.Shared.Lathe;
using Content.Shared.Research.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Lathe.UI;

/// <summary>
/// Initializes a <see cref="NuclearFabricatorWindow"/> and updates it when new server messages are received.
/// </summary>
/// <remarks>
/// Basically a 1:1 copy of <see cref="LatheBoundUserInterface"/> due to being unable to extend from it
/// </remarks>
[UsedImplicitly]
public sealed class NuclearFabricatorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NuclearFabricatorWindow? _menu;
    public NuclearFabricatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredRight<NuclearFabricatorWindow>();
        _menu.SetEntity(Owner);

        _menu.OnServerListButtonPressed += _ =>
        {
            SendMessage(new ConsoleServerSelectionMessage());
        };

        _menu.RecipeQueueAction += (recipe, amount) =>
        {
            SendMessage(new LatheQueueRecipeMessage(recipe, amount));
        };
        _menu.QueueDeleteAction += index => SendMessage(new LatheDeleteRequestMessage(index));
        _menu.QueueMoveUpAction += index => SendMessage(new LatheMoveRequestMessage(index, -1));
        _menu.QueueMoveDownAction += index => SendMessage(new LatheMoveRequestMessage(index, 1));
        _menu.DeleteFabricatingAction += () => SendMessage(new LatheAbortFabricationMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case LatheUpdateState msg:
                if (_menu != null)
                    _menu.Recipes = msg.Recipes;
                _menu?.PopulateRecipes();
                _menu?.UpdateCategories();
                _menu?.PopulateQueueList(msg.Queue);
                _menu?.SetQueueInfo(msg.CurrentlyProducing);
                break;
        }
    }
}