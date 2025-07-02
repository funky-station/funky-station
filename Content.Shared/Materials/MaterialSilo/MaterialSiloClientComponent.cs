using Robust.Shared.GameStates;

namespace Content.Shared.Materials.MaterialSilo;

/// <summary>
/// An entity with <see cref="MaterialStorageComponent"/> that interfaces with an <see cref="MaterialSiloComponent"/>.
/// Used for tracking the connected silo.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMaterialSiloSystem))]
public sealed partial class MaterialSiloClientComponent : Component
{
    /// <summary>
    /// The silo that this client pulls materials from.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Silo;
}
