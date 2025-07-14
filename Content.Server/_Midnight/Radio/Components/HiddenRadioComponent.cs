using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Radio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Radio.Components;

[RegisterComponent]
public sealed partial class HiddenRadioComponent : Component
{
    [DataField("channel", customTypeSerializer: typeof(PrototypeIdSerializer<RadioChannelPrototype>))]
    public string Channel = string.Empty;
    
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_RMC14/Effects/radiostatic.ogg");
}