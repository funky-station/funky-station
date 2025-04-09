using System.Linq;
using System.Numerics;
using Content.Server.Antag;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.EntityTable;
using Content.Shared.Physics;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.AlternateDimension;

public sealed partial class AlternateDimensionSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

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
        if (!EntityManager.TryGetComponent<MapGridComponent>(originalGridId, out var originalGridComp))
            return;

        // radius is based off the of the size of the map & can be adjusted in the .yml to taste
        var circleRadius = alternateGridComp.LocalAABB.Width * ent.Comp.RadiusModifier * 0.5f;

        var stationTiles = _mapSystem.GetAllTilesEnumerator(alternateGridId.Value, alternateGridComp);
        var alternateTiles = new List<(Vector2i Index, Tile Tile)>();

        var tilesRemoved = 0;
        var backupTile = new ValueTuple<Vector2i, Tile>(Vector2i.Zero, Tile.Empty);
        while (stationTiles.MoveNext(out var tileRef))
        {
            backupTile = (tileRef.Value.GridIndices, tileRef.Value.Tile); // in case this is configured wrong we need at least one guaranteed place to spawn
                                                                          // the portal back
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

        sawmill.Log(LogLevel.Debug, "Removed {0} tiles from mob spawn consideration around ({1},{2}) with radius = {3}",
            tilesRemoved,
            coords.Value.Position.X,
            coords.Value.Position.Y,
            circleRadius);

        if (TryComp<AlternateDimensionAutoPortalComponent>(ent, out var AutoPortalComp))
        {
            var portalTile = backupTile;
            if (alternateTiles.Count > 0)
            {
                portalTile = alternateTiles.RemoveSwap(_random.Next(alternateTiles.Count));
            }

            var otherEnt = SpawnAtPosition(AutoPortalComp.OtherSidePortal,
                originalGridComp.GridTileToLocal(portalTile.Item1));
            var otherOtherEnt = SpawnAtPosition("PortalBlue",
                alternateGridComp.GridTileToLocal(portalTile.Item1));
            _link.TryLink(otherEnt, otherOtherEnt, true);

            //Make sure
            if (TryComp<PortalComponent>(otherEnt, out var portal1))
            {
                portal1.CanTeleportToOtherMaps = true;
            }
            if (TryComp<PortalComponent>(otherOtherEnt, out var portal2))
            {
                portal2.CanTeleportToOtherMaps = true;
            }
        }

        var players = _antag.GetTotalPlayerCount(_player.Sessions);
        var count = Math.Clamp(Convert.ToInt32(players * ent.Comp.PlayerScaling), ent.Comp.Min, ent.Comp.Max);

        var spawns = 0;
        bool guarenteedSpawn = true;
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
            foreach (var spawn in _entityTable.GetSpawns(ent.Comp.Table))
            {
                var uid = EntityManager.SpawnAtPosition(spawn.Id, spawnCoords);

                EntityManager.RemoveComponent<GhostRoleComponent>(uid);
                EntityManager.RemoveComponent<GhostTakeoverAvailableComponent>(uid);
                spawns++;

                sawmill.Log(LogLevel.Debug,
                    "Spawned mob \"{0}\" at ({1},{2}), {3} tiles away from ({4},{5})",
                    spawn.Id,
                    spawnCoords.Position.X,
                    spawnCoords.Position.Y,
                    Vector2.Distance(coords.Value.Position, spawnCoords.Position),
                    coords.Value.Position.X,
                    coords.Value.Position.Y);
            }

            if (guarenteedSpawn)
            {
                guarenteedSpawn = false;

                if (ent.Comp.SpecialEntries.Count == 0)
                    continue;

                // guaranteed spawn
                var specialEntry = _random.Pick(ent.Comp.SpecialEntries);
                Spawn(specialEntry.PrototypeId, spawnCoords);
            }
        }
    }
}
