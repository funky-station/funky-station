using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Midnight.Storage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HiddenStorageComponent : Component
{
    /// <summary>
    /// Sound played when the hidden storage is opened
    /// </summary>
    [DataField("openSound")]
    public SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/Machines/screwdriveropen.ogg");

    /// <summary>
    /// Sound played when the hidden storage is closed
    /// </summary>
    [DataField("closeSound")]
    public SoundSpecifier CloseSound = new SoundPathSpecifier("/Audio/Machines/screwdriverclose.ogg");

    /// <summary>
    /// Amount of time in seconds it takes to open
    /// </summary>
    [DataField]
    public TimeSpan OpenDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The tool quality needed to open this panel.
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> OpeningTool = "Screwing";

    /// <summary>
    /// Whether the storage is currently open
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsOpen;
}