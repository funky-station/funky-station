namespace Content.Server._Goobstation.WeaponRandomExplode;

[RegisterComponent]
public sealed partial class WeaponRandomExplodeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float explosionChance;

    /// <summary>
    /// if not filled - the explosion force will be 1.
    /// if filled - the explosion force will be the current charge multiplied by this.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float multiplyByCharge;

    /// <summary>
    /// decreases the self damage and explosion radius
    /// </summary>
    [DataField, AutoNetworkedField]
    public float reduction;

    /// <summary>
    /// deletes the gun after the explosion if this is true
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool destroygun;
}
