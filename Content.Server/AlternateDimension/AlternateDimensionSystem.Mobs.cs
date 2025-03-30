using System.Linq;
using System.Numerics;
using Content.Server.Antag;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.AlternateDimension;

public sealed partial class AlternateDimensionSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private void InitializeMobs()
    {
        SubscribeLocalEvent<AlternateDimensionMobSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<AlternateDimensionMobSpawnerComponent> ent, ref MapInitEvent args)
    {
        //Mobs
        var sawmill = _logManager.GetSawmill("alternatedimension_mobs");
        var coords = GetAlternateRealityCoordinates(ent, ent.Comp.TargetDimension);
        if (coords is null)
            return;

        var originalGridId = _transform.GetGrid(ent.Owner);
        if (originalGridId is null)
            return;

        var alternateGridId = GetAlternateRealityGrid(originalGridId.Value, ent.Comp.TargetDimension);
        if (alternateGridId is null)
            return;
        if (!EntityManager.TryGetComponent<MapGridComponent>(alternateGridId, out var alternateGridComp))
            return;

        sawmill.Log(LogLevel.Warning, "Wowee!");

        // radius is based off the of the size of the map & can be adjusted in the .yml to taste
        var circleRadius = alternateGridComp.LocalAABB.Width * ent.Comp.RadiusModifier * 0.5f;

        var stationTiles = _mapSystem.GetAllTilesEnumerator(alternateGridId.Value, alternateGridComp);
        var alternateTiles = new List<(Vector2i Index, Tile Tile)>();

        var tilesRemoved = 0;
        while (stationTiles.MoveNext(out var tileRef))
        {
            var circle = new Circle(coords.Value.Position, circleRadius);
            var tileBox = new Box2(tileRef.Value.GridIndices * alternateGridComp.TileSize,
                (tileRef.Value.GridIndices + Vector2i.One) * alternateGridComp.TileSize);
            if (circle.Intersects(tileBox))
            {
                tilesRemoved++;
                continue;
            }

            alternateTiles.Add((tileRef.Value.GridIndices, tileRef.Value.Tile));
        }

        sawmill.Log(LogLevel.Debug, "Removed {0} tiles from mob spawn consideration.", tilesRemoved);

        var players = _antag.GetTotalPlayerCount(_player.Sessions);
        var count = Math.Clamp(Convert.ToInt32(players * ent.Comp.PlayerScaling), ent.Comp.Min, ent.Comp.Max);

        var spawns = 0;
        while (spawns < count)
        {
            if (alternateTiles.Count == 0)
                break;

            var tile = alternateTiles.RemoveSwap(_random.Next(alternateTiles.Count));

            if (!_anchorable.TileFree(alternateGridComp,
                    tile.Index,
                    (int) CollisionGroup.MachineLayer,
                    (int) CollisionGroup.MachineLayer))
            {
                continue;
            }

            var spawnCoords = alternateGridComp.GridTileToLocal(tile.Index);
            var uid = EntityManager.SpawnAtPosition("MobXenoDrone", spawnCoords);
            EntityManager.RemoveComponent<GhostRoleComponent>(uid);
            EntityManager.RemoveComponent<GhostTakeoverAvailableComponent>(uid);
            spawns++;

            sawmill.Log(LogLevel.Debug, "Spawned mob at ({0},{1}), {2} tiles away from ({3},{4})",
                spawnCoords.Position.X,
                spawnCoords.Position.Y,
                Vector2.Distance(coords.Value.Position, spawnCoords.Position),
                coords.Value.Position.X,
                coords.Value.Position.Y);
        }

    }
}
