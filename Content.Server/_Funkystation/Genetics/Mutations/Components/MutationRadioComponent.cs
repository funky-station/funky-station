using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Gives the user access to a given channel without the need for a headset.
/// </summary>
/// <remark>
/// Credit where due goes to RadioImplantComponent's author. Modified for use with the Meta-Neural Antenna mutation.
/// </remark>
[RegisterComponent]
public sealed partial class MutationRadioComponent : Component
{
    /// <summary>
    /// The radio channel(s) to grant access to.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<RadioChannelPrototype>> RadioChannels = new();

    /// <summary>
    /// The radio channels that have been added by the implant to a user's ActiveRadioComponent.
    /// Used to track which channels were successfully added (not already in user)
    /// </summary>
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> ActiveAddedChannels = new();

    /// <summary>
    /// The radio channels that have been added by the implant to a user's IntrinsicRadioTransmitterComponent.
    /// Used to track which channels were successfully added (not already in user)
    /// </summary>
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> TransmitterAddedChannels = new();
}
