// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Physics.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.SubFloor;
using Content.Shared.Pinpointer;
using Robust.Server.GameObjects;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Handles setting up the final Blood Cult summoning ritual site.
/// Finds a valid 2x3 space near a departmental beacon, replaces flooring, and spawns the rift with runes.
/// </summary>
public sealed class BloodCultRiftSetupSystem : EntitySystem
{
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

	/// <summary>
	/// Attempts to set up the final summoning ritual site.
	/// Returns the rift entity if successful, null otherwise.
	/// </summary>
	public EntityUid? TrySetupRitualSite(BloodCultRuleComponent cultRule)
	{
		// 1. Select a random departmental beacon
		var beacons = GetDepartmentalBeacons();
		if (beacons.Count == 0)
			return null;

		var targetBeacon = _random.Pick(beacons);
		var beaconCoords = Transform(targetBeacon).Coordinates;

		// 2. Find a valid 2x3 space near the beacon
		if (!TryFindValid2x3Space(beaconCoords, out var centerCoords, out var gridUid, out var grid) || grid == null)
			return null;

		// 3. Replace flooring with reinforced
		ReplaceFlooring(gridUid, grid, centerCoords);

		// 4. Spawn rift and runes
		var rift = SpawnRiftAndRunes(centerCoords);

		return rift;
	}

	private List<EntityUid> GetDepartmentalBeacons()
	{
		var beacons = new List<EntityUid>();
		var query = EntityQueryEnumerator<NavMapBeaconComponent, MetaDataComponent>();

		while (query.MoveNext(out var beaconUid, out var navBeacon, out var meta))
		{
			if (meta.EntityPrototype != null &&
				BloodCultRuleComponent.PossibleVeilLocations.Contains(meta.EntityPrototype.ID))
			{
				beacons.Add(beaconUid);
			}
		}

		return beacons;
	}

	private bool TryFindValid2x3Space(EntityCoordinates center, out EntityCoordinates validCenter,
		out EntityUid gridUid, out MapGridComponent? grid)
	{
		validCenter = EntityCoordinates.Invalid;
		gridUid = EntityUid.Invalid;
		grid = null;

	if (!TryComp<MapGridComponent>(center.EntityId, out grid))
		return false;

	gridUid = center.EntityId;
	var centerTile = _mapSystem.TileIndicesFor(gridUid, grid, center);

	// Search in a 10x10 area around the beacon
	for (var x = -5; x <= 5; x++)
	{
		for (var y = -5; y <= 5; y++)
		{
			var testTile = new Vector2i(centerTile.X + x, centerTile.Y + y);

			if (IsValid2x3Space(gridUid, grid, testTile))
			{
				validCenter = _mapSystem.GridTileToLocal(gridUid, grid, testTile);
				return true;
			}
		}
	}

		return false;
	}

	private bool IsValid2x3Space(EntityUid gridUid, MapGridComponent grid, Vector2i bottomLeft)
	{
		// Check a 2-wide by 3-tall area
		for (var x = 0; x < 2; x++)
		{
			for (var y = 0; y < 3; y++)
			{
				var checkTile = new Vector2i(bottomLeft.X + x, bottomLeft.Y + y);

				if (!IsTileValid(gridUid, grid, checkTile))
					return false;
			}
		}

		return true;
	}

	private bool IsTileValid(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
	{
		// Check if tile exists and is not space
		var tileRef = _mapSystem.GetTileRef(gridUid, grid, tile);
		if (tileRef.Tile.IsEmpty)
			return false;

		// Check for blocking entities (walls, etc)
		var anchored = _mapSystem.GetAnchoredEntities(gridUid, grid, tile);
		foreach (var entity in anchored)
		{
			// Allow subfloor items (cables, pipes)
			if (HasComp<SubFloorHideComponent>(entity))
				continue;

			// Block on walls or dense structures
			if (TryComp<PhysicsComponent>(entity, out var physics))
			{
				if ((physics.CollisionLayer & (int)CollisionGroup.Impassable) != 0)
					return false;
			}
		}

		return true;
	}

	private void ReplaceFlooring(EntityUid gridUid, MapGridComponent grid, EntityCoordinates center)
	{
		var centerTile = _mapSystem.TileIndicesFor(gridUid, grid, center);

		// Get reinforced floor tile
		var reinforcedTileDef = (ContentTileDefinition)_tileDefManager["FloorReinforced"];
		var reinforcedTile = new Tile(reinforcedTileDef.TileId);

		// Replace 2x3 area with reinforced flooring (centered around the rift position)
		for (var x = -1; x <= 0; x++)
		{
			for (var y = -1; y <= 1; y++)
			{
				var tileIndices = new Vector2i(centerTile.X + x, centerTile.Y + y);
				_mapSystem.SetTile(gridUid, grid, tileIndices, reinforcedTile);
			}
		}
	}

	private EntityUid SpawnRiftAndRunes(EntityCoordinates center)
	{
		// Spawn rift at center
		var rift = Spawn("BloodCultRift", center);
		var riftComp = EnsureComp<BloodCultRiftComponent>(rift);

		// Spawn 3 runes: left (-1, 0), right (1, 0), bottom (0, -1)
		var leftRune = Spawn("TearVeilRune", center.Offset(new Vector2(-1, 0)));
		var rightRune = Spawn("TearVeilRune", center.Offset(new Vector2(1, 0)));
		var bottomRune = Spawn("TearVeilRune", center.Offset(new Vector2(0, -1)));

		// Mark as final summoning runes
		var leftFinal = EnsureComp<FinalSummoningRuneComponent>(leftRune);
		var rightFinal = EnsureComp<FinalSummoningRuneComponent>(rightRune);
		var bottomFinal = EnsureComp<FinalSummoningRuneComponent>(bottomRune);

		leftFinal.RiftUid = rift;
		rightFinal.RiftUid = rift;
		bottomFinal.RiftUid = rift;

		riftComp.SummoningRunes.Add(leftRune);
		riftComp.SummoningRunes.Add(rightRune);
		riftComp.SummoningRunes.Add(bottomRune);

		return rift;
	}
}

