using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationInkGlandsComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ActionId = "ActionInkSpurt";

    public EntityUid? GrantedAction;

    [DataField]
    public int Amount = 10;

    [DataField]
    public SoundSpecifier SpillSound = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
}
