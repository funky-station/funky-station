using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Paper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BookPaginationComponent : Component
{
    [DataField, AutoNetworkedField]
    public int CurrentPage;

    [DataField]
    public int LinesPerPage = 19;
}
