using System.Numerics;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Photography;

public sealed partial class PhotographySystem
{
    [Dependency] private readonly MapSystem _mapSys = default!;

    /// <summary>
    ///     Separation between photos in the PhotoMap
    /// </summary>
    private const int SceneSeperation = 100;

    /// <summary>
    ///     Map where all photo scenes reside.
    /// </summary>
    public EntityUid? SceneMap { get; private set; }

    /// <summary>
    ///     The number of photo scenes created in the map.
    ///     Used for calculating the position of the next one.
    /// </summary>
    private int _photoScenes = 0;

    /// <summary>
    ///     Ensures that the tabletop map exists. Creates it if it doesn't.
    /// </summary>
    private void EnsureSceneMap()
    {
        if (SceneMap != null)
            return;

        if (TryComp<MapComponent>(SceneMap, out _))
            return;

        SceneMap = _mapSys.CreateMap(false); // do not run map init we dont want entities acting up in here

        if (!TryComp<MapComponent>(SceneMap, out var mapComponent) && SceneMap == null)
        {
            throw new Exception("Failed to create map, somehow?");
        }

        // Lighting is always disabled in tabletop world.
        mapComponent!.LightingEnabled = false;
        Dirty((EntityUid) SceneMap, mapComponent);
    }

    /// <summary>
    ///     Gets the next available position for a tabletop, and increments the tabletop count.
    ///     Taken from TabletopSystem.Map.cs.
    /// </summary>
    /// <returns></returns>
    private Vector2 GetNextTabletopPosition()
    {
        return UlamSpiral(_photoScenes++) * SceneSeperation;
    }

    /// <summary>
    ///     Algorithm for mapping scalars to 2D positions in the same pattern as an Ulam Spiral.
    ///     Taken from TabletopSystem.Map.cs.
    /// </summary>
    /// <param name="n">Scalar to map to a 2D position.</param>
    /// <returns>The mapped 2D position for the scalar.</returns>
    private Vector2i UlamSpiral(int n)
    {
        var k = (int)MathF.Ceiling(MathF.Sqrt(n) - 1) / 2;
        var t = 2 * k + 1;
        var m = (int)MathF.Pow(t, 2);
        t--;

        if (n >= m - t)
            return new Vector2i(k - (m - n), -k);

        m -= t;

        if (n >= m - t)
            return new Vector2i(-k, -k + (m - n));

        m -= t;

        return n >= m - t ? new Vector2i(-k + (m - n), k) : new Vector2i(k, k - (m - n - t));
    }
}
