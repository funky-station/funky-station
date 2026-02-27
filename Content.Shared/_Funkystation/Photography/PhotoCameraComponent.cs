using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Funkystation.Photography;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhotoCameraComponent : Component
{
    [ViewVariables, DataField]
    public bool Flash = false;

    [DataField("action", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Action = "ActionCapturePhoto";

    [DataField("actionEntity")]
    public EntityUid? ActionEntity;
}

public sealed partial class PhotoCameraTakePictureEvent : InstantActionEvent
{
    public bool Selfie = false;
}
