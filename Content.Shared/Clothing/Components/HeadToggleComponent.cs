using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(HeadToggleSystem))]
public sealed partial class HeadToggleComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ToggleAction = "ActionToggleHead";

    /// <summary>
    /// The action entity for toggling this item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField, AutoNetworkedField]
    public bool IsToggled;

    /// <summary>
    /// Equipped prefix to use after the helmet/visor is toggled.
    /// For a welding helmet, this is usually "up".
    /// </summary>
    [DataField, AutoNetworkedField]
    public string EquippedPrefix = "up";

    /// <summary>
    /// When <see langword="true"/> will function normally, otherwise will not react to events
    /// </summary>
    [DataField("enabled"), AutoNetworkedField]
    public bool IsEnabled = true;
}
