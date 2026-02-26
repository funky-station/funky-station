using Robust.Shared.GameStates;

namespace Content.Server._Funkystation.Manifest;

[RegisterComponent]
public sealed partial class ManifestInfoComponent : Component
{
    ///<summary>
    ///     The last message sent by an entity possessed by the mind.
    ///</summary>
    [DataField]
    public string? LastMessage { get; set; }

    ///<summary>
    ///     The last entity this mind has possessed.
    ///     Currently used to display the correct "image" of the player,
    ///     if the player has been borged, for example.
    ///</summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LastEntity { get; set; }
}

