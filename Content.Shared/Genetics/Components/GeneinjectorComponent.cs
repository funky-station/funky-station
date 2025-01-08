using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGeneSystem))]
public sealed partial class GeneinjectorComponent : Component
{
    /// <summary>
    ///     The time it takes to cuff an entity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 2.5f;

    /// <summary>
    ///     If an entity being cuffed is stunned, this amount of time is subtracted from the time it takes to add/remove their cuffs.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 2f;

    /// <summary>
    /// Whether the cuffs are currently being used to cuff someone.
    /// We need the extra information for when the virtual item is deleted because that can happen when you simply stop
    /// pulling them on the ground.
    /// </summary>
    [DataField]
    public bool Used;

    /// <summary>
    ///     The iconstate used with the RSI file for the player cuffed overlay.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string? BodyIconState = "body-overlay";

    /// <summary>
    /// An opptional color specification for <see cref="BodyIconState"/>
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Color Color = Color.White;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier StartGeneSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg");

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier EndGeneSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_end.ogg");
}

