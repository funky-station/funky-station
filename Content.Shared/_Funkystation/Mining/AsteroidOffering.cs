using Content.Shared.Procedural;

namespace Content.Shared._Funkystation.Mining;

/// <summary>
/// Asteroid offered for the magnet.
/// </summary>
public record struct AsteroidOffering : IMiningMagnetOffering
{
    public string Id;

    public DungeonConfig DungeonConfig;

    /// <summary>
    /// Calculated marker layers for the asteroid.
    /// </summary>
    public Dictionary<string, int> MarkerLayers;
}
