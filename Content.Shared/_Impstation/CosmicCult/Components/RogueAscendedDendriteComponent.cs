using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Impstation.CosmicCult.Components;

/// <summary>
/// Component for Ascendant's Dendrite for the reward system.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RogueAscendedDendriteComponent : Component
{
    /*[DataField] public SoundSpecifier ActivateSfx = new SoundPathSpecifier("/Audio/_Impstation/CosmicCult/ability_nova_impact.ogg");
    [DataField] public EntProtoId Vfx = "CosmicGenericVFX";
    [DataField, AutoNetworkedField] public TimeSpan StunTime = TimeSpan.FromSeconds(2);
    [DataField] public EntProtoId RogueFoodAction = "ActionRogueCosmicNova";
    [DataField] public EntityUid? RogueFoodActionEntity;*/

    // funky: change it to an organ
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? Action = "ActionRogueCosmicNova";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
