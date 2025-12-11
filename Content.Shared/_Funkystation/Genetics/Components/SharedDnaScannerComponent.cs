using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Genetics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DnaScannerComponent : Component
{
    public const string BodyContainerId = "body";

    [ViewVariables(VVAccess.ReadWrite)]
    public ContainerSlot BodyContainer { get; internal set; } = default!;

    [DataField("entryDelay"), ViewVariables(VVAccess.ReadWrite)]
    public float EntryDelay { get; private set; } = 2f;

    [ViewVariables]
    public EntityUid? Occupant => BodyContainer.ContainedEntity;

    public bool IsOccupied => Occupant != null;

    // Kept for potential future use

    // [DataField("locked"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    // public bool Locked { get; set; } = false;

    // [DataField("permaLocked"), ViewVariables(VVAccess.ReadWrite)]
    // public bool PermaLocked { get; private set; } = false;
}

[RegisterComponent]
public sealed partial class DnaScannerVisualsComponent : Component
{

}

[NetSerializable, Serializable]
public enum DnaScannerVisualLayers : byte
{
    Main
}

[Serializable, NetSerializable]
public enum DnaScannerVisuals : byte
{
    State,
}

[NetSerializable, Serializable]
public enum DnaScannerState : byte
{
    Empty,
    Occupied,
    EmptyUnpowered,
    OccupiedUnpowered
}
