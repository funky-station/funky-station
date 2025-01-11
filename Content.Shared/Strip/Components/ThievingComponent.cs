using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared.Strip.Components;

/// <summary>
/// Give this to an entity when you want to decrease stripping times
/// </summary>
[RegisterComponent]
public sealed partial class ThievingComponent : Component
{
    /// <summary>
    /// How much the strip time should be shortened by
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("stripTimeReduction")]
    public TimeSpan StripTimeReduction = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    /// The preset stripTimeReduction
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DefaultTimeReduction; // funkystation

    /// <summary>
    /// Should it notify the user if they're stripping a pocket?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("stealthy")]
    public bool Stealthy;

    [ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<AlertPrototype> ThievingAlertProtoId = "Thieving";
}

public sealed partial class ThievingToggleEvent : BaseAlertEvent; // funkystation
