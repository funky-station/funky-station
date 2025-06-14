using Content.Shared.Whitelist;

namespace Content.Shared.Armor;

/// <summary>
///     Used on outerclothing to allow use of tank storage
/// </summary>
[RegisterComponent]
public sealed partial class AllowTankStorageComponent : Component
{
    /// <summary>
    /// Whitelist for what entities are allowed in the tank storage slot.
    /// </summary>
    [DataField]
    public EntityWhitelist Whitelist = new()
    {
        Components = new[] {"Item"}
    };
}
