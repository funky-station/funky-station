using Content.Shared.Whitelist;

namespace Content.Shared.Armor;

/// <summary>
///     Used on outerclothing to allow use of weapon storage
/// </summary>
[RegisterComponent]
public sealed partial class AllowBackStorageComponent : Component
{
    /// <summary>
    /// Whitelist for what entities are allowed in the weapon storage slot.
    /// </summary>
    [DataField]
    public EntityWhitelist Whitelist = new()
    {
        Components = new[] {"Item"}
    };
}
