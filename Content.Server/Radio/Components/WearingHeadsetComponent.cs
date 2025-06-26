using System.Linq;
using Content.Shared.Radio.Components;

namespace Content.Server.Radio.Components;

/// <summary>
/// Added to entities that are wearing headsets.
/// Allows speaking into radio channels.
/// Modified to support multiple headsets simultaneously.
/// </summary>
[RegisterComponent]
public sealed partial class WearingHeadsetComponent : Component
{
    /// <summary>
    /// All headsets currently being worn by this entity.
    /// Changed from single EntityUid to HashSet to support multiple headsets.
    /// </summary>
    [DataField("headsets")]
    public HashSet<EntityUid> Headsets = new();
    
    /// <summary>
    /// Legacy property for backwards compatibility.
    /// Returns the first headset or null if none equipped.
    /// </summary>
    public EntityUid? Headset
    {
        get => Headsets.FirstOrDefault();
        set
        {
            if (value.HasValue)
                Headsets.Add(value.Value);
        }
    }
}