using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Tools.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGeneinjectorSystem))]
public sealed partial class GeneinjectorComponent : Component
{
    /// <summary>
    ///     The time it takes to cuff an entity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 5f;

    /// <summary>
    ///     If an entity being cuffed is stunned, this amount of time is subtracted from the time it takes to add/remove their cuffs.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 2f;

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
