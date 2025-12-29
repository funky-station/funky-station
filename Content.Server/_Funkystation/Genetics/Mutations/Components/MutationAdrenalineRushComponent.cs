using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationAdrenalineRushComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ActionId = "ActionAdrenalineRush";

    public EntityUid? GrantedAction;
}
