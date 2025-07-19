// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using System.Numerics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.FixedPoint;
using Content.Server.Radiation.Components;
using Content.Shared.BloodCult;
using Content.Server.BloodCult.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.Systems;
using Content.Shared.Stacks;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Collections;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.BloodCult.Systems;

// Most of this is reused radiation code. Some radiation blocker checks have been removed.
public sealed partial class CultHealingSourceSystem : EntitySystem
{
	// Raycast timing accumulator
	private float _accumulator;

	// CCVar Storage
	public float MinIntensity { get; private set; }
	public float GridcastUpdateRate { get; private set; }
	public bool GridcastSimplifiedSameGrid { get; private set; }
	public float GridcastMaxDistance { get; private set; }

	// Dependencies
	[Dependency] private readonly SharedStackSystem _stack = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	/// <summary>
	/// 	Subscribe to the cult healing system's server CCVars.
	/// </summary>
	private void SubscribeCvars()
    {
        Subs.CVar(_cfg, CCVars.CultHealingMinIntensity, cultHealingMinIntensity => MinIntensity = cultHealingMinIntensity, true);
        Subs.CVar(_cfg, CCVars.CultHealingGridcastUpdateRate, updateRate => GridcastUpdateRate = updateRate, true);
        Subs.CVar(_cfg, CCVars.CultHealingGridcastSimplifiedSameGrid, simplifiedSameGrid => GridcastSimplifiedSameGrid = simplifiedSameGrid, true);
        Subs.CVar(_cfg, CCVars.CultHealingGridcastMaxDistance, maxDistance => GridcastMaxDistance = maxDistance, true);
    }

	public override void Initialize()
	{
		base.Initialize();
		SubscribeCvars();
		SubscribeLocalEvent<CultHealingSourceComponent, AnchorStateChangedEvent>(OnAnchorChanged);
		// Just borrowing the radiation blocker system -- the same blockers
		// 	will work fine for my purposes here. No need to double up.
		// InitRadBlocking();
	}

	public override void Update(float frameTime)
	{
		base.Update(frameTime);

		_accumulator += frameTime;
		if (_accumulator < GridcastUpdateRate)
			return;

		UpdateGridcast();
		_accumulator = 0f;
	}

	private void OnAnchorChanged(EntityUid uid, CultHealingSourceComponent component, AnchorStateChangedEvent args)
	{
		if (args.Anchored)
			component.Enabled = true;
		else
			component.Enabled = false;
	}

	public void UpdateGridcast()
	{
		var saveVisitedTiles = false;//_debugSessions.Count > 0;

		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var sources = EntityQueryEnumerator<CultHealingSourceComponent, TransformComponent>();
		var destinations = EntityQueryEnumerator<BloodCultistComponent, TransformComponent>();
		var destinations_constructs = EntityQueryEnumerator<BloodCultConstructComponent, TransformComponent>();
		var resistanceQuery = GetEntityQuery<RadiationGridResistanceComponent>();
		var transformQuery = GetEntityQuery<TransformComponent>();
		var gridQuery = GetEntityQuery<MapGridComponent>();
		var stackQuery = GetEntityQuery<StackComponent>();

		// precalculate world positions for each source
        // so we won't need to calc this in cycle over and over again
        var sourcesData = new ValueList<(EntityUid, CultHealingSourceComponent, TransformComponent, Vector2)>();
        while (sources.MoveNext(out var uid, out var source, out var sourceTrs))
        {
            if (!source.Enabled)
                continue;

            var worldPos = _transform.GetWorldPosition(sourceTrs, transformQuery);
            var data = (uid, source, sourceTrs, worldPos);
            sourcesData.Add(data);
        }

		// trace all rays from cult healing source to cultists
		var rays = new List<CultHealingRay>();
		var receiversTotalHealing = new ValueList<(Entity<BloodCultistComponent>, float)>();
		while (destinations.MoveNext(out var destUid, out var dest, out var destTrs))
		{
			// For all blood cultists...
			if (_mobState.IsDead(destUid))
				continue;  // Do not heal the dead.

			var destWorld = _transform.GetWorldPosition(destTrs, transformQuery);

			bool inRange = false;
			var healing = 0f;
			foreach(var (uid, source, sourceTrs, sourceWorld) in sourcesData)
			{
				// For all cult healers...
				stackQuery.TryGetComponent(uid, out var stack);
				var intensity = source.Intensity * _stack.GetCount(uid, stack);

				// send ray towards destination entity
				var ray = CultHeal(uid, sourceTrs, sourceWorld, destUid,
					destTrs, destWorld, intensity, source.Slope,
					saveVisitedTiles, resistanceQuery, transformQuery,
					gridQuery);
				if (ray == null)
					continue;

				// save ray for debug
				rays.Add(ray);

				// add healing to total exposure
				if (ray.ReachedDestination)
				{
					healing += ray.Healing;
					inRange = true;
				}
			}
			receiversTotalHealing.Add( ((destUid, dest), healing) );
			if (inRange)
				CultReconvertEntity(destUid);  // reconvert in-range entities by a flat rate
		}

		var receiversTotalHealingConstructs = new ValueList<(Entity<BloodCultConstructComponent>, float)>();
		while (destinations_constructs.MoveNext(out var destUid, out var dest, out var destTrs))
		{
			// For all cult constructs...
			if (_mobState.IsDead(destUid))
				continue;  // Do not heal the dead.

			var destWorld = _transform.GetWorldPosition(destTrs, transformQuery);

			var healing = 0f;
			foreach(var (uid, source, sourceTrs, sourceWorld) in sourcesData)
			{
				// For all cult healers...
				stackQuery.TryGetComponent(uid, out var stack);
				var intensity = source.Intensity * _stack.GetCount(uid, stack);

				// send ray towards destination entity
				var ray = CultHeal(uid, sourceTrs, sourceWorld, destUid,
					destTrs, destWorld, intensity, source.Slope,
					saveVisitedTiles, resistanceQuery, transformQuery,
					gridQuery);
				if (ray == null)
					continue;

				// save ray for debug
				rays.Add(ray);

				// add healing to total exposure
				if (ray.ReachedDestination)
					healing += ray.Healing;
			}
			receiversTotalHealingConstructs.Add( ((destUid, dest), healing) );
		}

		// send healing to each entity
		foreach (var (receiver, healing) in receiversTotalHealing)
		{
			if (healing > 0)
				CultHealEntity(receiver, healing, GridcastUpdateRate);
		}
		foreach (var (receiver, healing) in receiversTotalHealingConstructs)
		{
			if (healing > 0)
				CultHealEntity(receiver, healing, GridcastUpdateRate);
		}

		//RaiseLocalEvent(new CultHealingSystemUpdatedEvent());
	}

	private CultHealingRay? CultHeal(EntityUid sourceUid, TransformComponent sourceTrs, Vector2 sourceWorld,
        EntityUid destUid, TransformComponent destTrs, Vector2 destWorld,
        float incomingHealth, float slope, bool saveVisitedTiles,
        EntityQuery<RadiationGridResistanceComponent> resistanceQuery,
        EntityQuery<TransformComponent> transformQuery, EntityQuery<MapGridComponent> gridQuery)
	{
		// are they on the same map
		if (sourceTrs.MapID != destTrs.MapID)
			return null;

		var mapId = sourceTrs.MapID;

		// get direction from healing source to destination and its distance
		var dir = destWorld - sourceWorld;
		var dist = dir.Length();

		// check if receiver is too far away
		if (dist > GridcastMaxDistance)
			return null;

		// will it even reach destination considering distance penalty
		var healing = incomingHealth - slope * dist;

		if (healing <= MinIntensity)
			return null;

		// create a new cult healing ray from source to destination
		// initially assume that it doesn't hit any radiation blockers
		var ray = new CultHealingRay(mapId, GetNetEntity(sourceUid), sourceWorld, GetNetEntity(destUid), destWorld, healing);

		// if source and destination are on the same grid then technically
		// there might be a grid between them as well, but just like the
		// radiation system, we can ignore that because who cares heehoo
		if (GridcastSimplifiedSameGrid && sourceTrs.GridUid != null && sourceTrs.GridUid == destTrs.GridUid)
		{
			if (!gridQuery.TryGetComponent(sourceTrs.GridUid.Value, out var gridComponent))
				return ray;
			return Gridcast((sourceTrs.GridUid.Value, gridComponent), ray, saveVisitedTiles, resistanceQuery, sourceTrs,
				destTrs, transformQuery.GetComponent(sourceTrs.GridUid.Value));
		}

		// okay, so we're doing it the hard way.
		// don't say I didn't warn you.
		// seriously you should turn GridcastSimplifiedSameGrid on.

		// check how many grids are between the source and destination
		// do a box intersection test between target and destination
		var box = Box2.FromTwoPoints(sourceWorld, destWorld);
		var grids = new List<Entity<MapGridComponent>>();
		_mapManager.FindGridsIntersecting(mapId, box, ref grids, true);

		// gridcast through each grid to check for radiation blockers
		foreach (var grid in grids)
		{
			ray = Gridcast(grid, ray, saveVisitedTiles, resistanceQuery, sourceTrs, destTrs,
				transformQuery.GetComponent(grid));

			// check to see if we have exhausted the healing ray's potential
			if (ray.Healing <= 0)
				return ray;
		}

		return ray;
	}

	private CultHealingRay Gridcast(Entity<MapGridComponent> grid, CultHealingRay ray, bool saveVisitedTiles,
        EntityQuery<RadiationGridResistanceComponent> resistanceQuery,
        TransformComponent sourceTrs,
        TransformComponent destTrs,
        TransformComponent gridTrs)
	{
		var blockers = new List<(Vector2i, float)>();

		// if grid doesn't have resistance map just apply distance penalty
		var gridUid = grid.Owner;
		if (!resistanceQuery.TryGetComponent(gridUid, out var resistance))
			return ray;
		var resistanceMap = resistance.ResistancePerTile;

		// get coordinates of source and destination in grid coordinates
		Vector2 srcLocal = sourceTrs.ParentUid == grid.Owner
            ? sourceTrs.LocalPosition
            : Vector2.Transform(ray.Source, gridTrs.InvLocalMatrix);

        Vector2 dstLocal = destTrs.ParentUid == grid.Owner
            ? destTrs.LocalPosition
            : Vector2.Transform(ray.Destination, gridTrs.InvLocalMatrix);

        Vector2i sourceGrid = new(
            (int) Math.Floor(srcLocal.X / grid.Comp.TileSize),
            (int) Math.Floor(srcLocal.Y / grid.Comp.TileSize));

        Vector2i destGrid = new(
            (int) Math.Floor(dstLocal.X / grid.Comp.TileSize),
            (int) Math.Floor(dstLocal.Y / grid.Comp.TileSize));

		// iterate through the tiles in the grid line from source to destination
		var line = new GridLineEnumerator(sourceGrid, destGrid);
		while (line.MoveNext())
		{
			var point = line.Current;
			if (resistanceMap.TryGetValue(point, out var resData))
				continue;
			ray.Healing -= resData;

			// save data for debug
			if (saveVisitedTiles)
				blockers.Add((point, ray.Healing));

			// no intensity left after blocker
			if (ray.Healing <= MinIntensity)
			{
				ray.Healing = 0;
				break;
			}
		}

		// save data for debug if needed
        if (saveVisitedTiles && blockers.Count > 0)
            ray.Blockers.Add(GetNetEntity(gridUid), blockers);

		return ray;
	}

	public void CultHealEntity(EntityUid uid, float healingPerSecond, float time)
	{
		if (TryComp<DamageableComponent>(uid, out var damageable))
		{
			var keys = new List<string>();
			
			foreach (var item in damageable.Damage.DamageDict)
			{
				if (item.Value > 0)
					keys.Add(item.Key);
			}
			if (keys.Count == 0)
				return;
			
			var ds = new DamageSpecifier();
			foreach (var key in keys)
			{
				ds.DamageDict.Add(key, FixedPoint2.New(-(healingPerSecond * time) / keys.Count));
			}
			_damageableSystem.TryChangeDamage(uid, ds, true, false, origin: uid);
		}
	}

	public void CultReconvertEntity(EntityUid uid)
	{
		if (TryComp<BloodCultistComponent>(uid, out var bloodCultist))
		{
			if (bloodCultist.DeCultification > 0.0f)
				bloodCultist.DeCultification = bloodCultist.DeCultification - 1.0f;
			if (bloodCultist.DeCultification < 0.0f)
				bloodCultist.DeCultification = 0.0f;
		}
	}

	public void SetSourceEnabled(Entity<CultHealingSourceComponent?> entity, bool val)
	{
		if (!Resolve(entity, ref entity.Comp, false))
			return;

		entity.Comp.Enabled = val;
	}
}
