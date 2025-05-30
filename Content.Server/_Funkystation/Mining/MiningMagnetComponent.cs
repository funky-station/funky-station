namespace Content.Server._Funkystation.Mining;

[RegisterComponent]
public sealed partial class MiningMagnetComponent : Component
{
    /// <summary>
    /// The max distance at which the magnet will pull in wrecks.
    /// Scales from 50% to 100%.
    /// </summary>
    [DataField]
    public float MagnetSpawnDistance = 32f;

    /// <summary>
    /// How far offset to either side will the magnet wreck spawn.
    /// </summary>
    [DataField]
    public float LateralOffset = 1f;
}
