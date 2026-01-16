using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationSpatialDestabilizationComponent : Component
{
    [DataField]
    public float MinInterval = 60f;

    [DataField]
    public float MaxInterval = 120f;

    [DataField]
    public float TeleportRadius = 5f;

    [DataField]
    public int TeleportAttempts = 10;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");

    [ViewVariables]
    public TimeSpan NextTeleportTime;
}
