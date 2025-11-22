// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System;
using Content.Server.Stack;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Trigger;
using Content.Shared.Stacks;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.SubFloor;
using Content.Shared.Fluids.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Content.Server.BloodCult;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class SummonOnTriggerSystem : EntitySystem
{
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly EntityLookupSystem _lookup = default!;
	[Dependency] private readonly StackSystem _stackSystem = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	private EntityQuery<PhysicsComponent> _physicsQuery = default!;

	private const int JuggernautMetalRequired = 30;
	private const int PylonGlassRequired = 10;

	public override void Initialize()
	{
		base.Initialize();
		SubscribeLocalEvent<SummonOnTriggerComponent, TriggerEvent>(HandleSummonTrigger);

		_physicsQuery = GetEntityQuery<PhysicsComponent>();
	}

	private void HandleSummonTrigger(EntityUid uid, SummonOnTriggerComponent component, TriggerEvent args)
	{
		if (args.Handled || args.User == null)
			return;

		EntityUid user = (EntityUid)args.User;

		if (!TryComp<BloodCultistComponent>(user, out var bloodCultist))
			return;

		var runeCoords = Transform(uid).Coordinates;

		// Get all stacks in range - use Uncontained flag to exclude items in containers
		// Use a range that covers the entire tile plus a bit extra to catch all items on the rune
		var summonLookup = _lookup.GetEntitiesInRange(uid, component.SummonRange, LookupFlags.Uncontained);

		// Also check for anchored entities on the same tile (stacks shouldn't be anchored, but be thorough)
		var gridUid = _transform.GetGrid(runeCoords);
		if (gridUid != null && TryComp<MapGridComponent>(gridUid, out var grid))
		{
			var tileIndices = _mapSystem.TileIndicesFor(gridUid.Value, grid, runeCoords);
			var anchoredEntities = _mapSystem.GetAnchoredEntities(gridUid.Value, grid, tileIndices);
			foreach (var anchoredEntity in anchoredEntities)
			{
				if (TryComp<StackComponent>(anchoredEntity, out var anchoredStack))
				{
					// Add anchored stacks to the lookup results
					summonLookup.Add(anchoredEntity);
				}
			}
		}

		// Find all stacks in range
		List<EntityUid> runedMetalStacks = new List<EntityUid>();
		List<EntityUid> runedGlassStacks = new List<EntityUid>();
		//List<EntityUid> runedPlasteelStacks = new List<EntityUid>(); // Future use

		foreach (var entity in summonLookup)
		{
			if (!TryComp<StackComponent>(entity, out var stack))
				continue;

			if (stack.StackTypeId == "RunedMetal")
				runedMetalStacks.Add(entity);
			else if (stack.StackTypeId == "RunedGlass")
				runedGlassStacks.Add(entity);
			//else if (stack.StackTypeId == "RunedPlasteel")
			//	runedPlasteelStacks.Add(entity);
		}

		// Check for 30 runedmetal - spawn Juggernaut shell
		// First check if enough materials exist (without consuming)
		if (HasEnoughMaterials(runedMetalStacks, JuggernautMetalRequired))
		{
			// Only consume materials AFTER validation passes
			if (TryConsumeMaterials(runedMetalStacks, JuggernautMetalRequired, user))
			{
				var juggernautShell = Spawn("CultJuggernautShell", runeCoords);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-juggernaut-shell"),
					user, user, PopupType.Large
				);
				args.Handled = true;
				return;
			}
		}

		// Check for 10 runedglass - spawn Pylon anchored
		// First check if enough materials exist (without consuming)
		if (HasEnoughMaterials(runedGlassStacks, PylonGlassRequired))
		{
			// Perform ALL validation checks BEFORE consuming materials
			// This ensures materials are never lost if summoning fails
			
			// First check: Verify we have a valid grid
			var pylonGridUid = _transform.GetGrid(runeCoords);
			if (pylonGridUid == null)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon-no-grid"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}

			if (!TryComp<MapGridComponent>(pylonGridUid, out var pylonGrid))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon-no-grid"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}

			// Second check: Verify location is free of hard physics collision objects (players, mobs, etc)
			// The rune itself will be replaced by the pylon, so we only need to check for blocking entities
			if (!IsRuneLocationFree(runeCoords, uid))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon-location-blocked"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}

			// Get tile indices for anchoring (validate now, use later)
			var targetTile = _mapSystem.TileIndicesFor(pylonGridUid.Value, pylonGrid, runeCoords);

			// ALL VALIDATIONS PASSED - now try to spawn the pylon (don't consume materials yet)
			// Store the rune's transform information so we can preserve it
			var runeXform = Transform(uid);
			var runeCoordsForPylon = runeXform.Coordinates;
			var runeRotation = runeXform.LocalRotation;

			// Unanchor the rune first to free up the snapgrid cell immediately
			if (runeXform.Anchored)
			{
				_transform.Unanchor(uid, runeXform);
			}

			// Delete the rune immediately - this frees the snapgrid cell for the pylon
			// Use EntityManager.DeleteEntity for immediate deletion
			EntityManager.DeleteEntity(uid);

			// Now spawn the pylon normally - it will auto-anchor since CultPylon has anchored: true
			// The rune is already deleted so there's no conflict
			EntityUid? pylon = null;
			try
			{
				pylon = Spawn("CultPylon", runeCoordsForPylon);

				// Verify pylon was created successfully
				if (pylon == EntityUid.Invalid || !Exists(pylon.Value))
				{
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Verify pylon exists and has transform
				if (!TryComp<TransformComponent>(pylon.Value, out var pylonXform))
				{
					QueueDel(pylon.Value);
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Set the rotation to match the rune
				pylonXform.LocalRotation = runeRotation;

				// Verify pylon is actually anchored (it should be since CultPylon has anchored: true)
				// Refresh transform to get latest state
				if (!pylonXform.Anchored || !Exists(pylon.Value))
				{
					// Pylon didn't anchor or was deleted - clean up
					if (Exists(pylon.Value))
						QueueDel(pylon.Value);
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Double-check pylon still exists after a moment (in case something deleted it)
				if (!Exists(pylon.Value) || !TryComp<TransformComponent>(pylon.Value, out var pylonXformCheck) || !pylonXformCheck.Anchored)
				{
					if (Exists(pylon.Value))
						QueueDel(pylon.Value);
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Pylon is successfully spawned and anchored - NOW consume materials
				if (!TryConsumeMaterials(runedGlassStacks, PylonGlassRequired, user))
				{
					// This shouldn't happen since we already checked, but be safe
					QueueDel(pylon.Value);
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Success - pylon is spawned and anchored
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon"),
					user, user, PopupType.Large
				);
				args.Handled = true;
				return;
			}
			catch (Exception ex)
			{
				// Exception during pylon spawning - clean up
				if (pylon != null && Exists(pylon.Value))
					QueueDel(pylon.Value);

				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon-failed"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}
		}

		// TODO: Add runedplasteel handling here
		//if (TryConsumeMaterials(runedPlasteelStacks, requiredAmount, user))
		//{
		//	// Spawn something for runedplasteel
		//	args.Handled = true;
		//	return;
		//}

		// No valid materials found
		if (runedMetalStacks.Count == 0 && runedGlassStacks.Count == 0)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-summoning-no-materials"),
				user, user, PopupType.MediumCaution
			);
		}
		else
		{
			// Calculate what materials are present and what's missing
			int totalMetal = 0;
			int totalGlass = 0;
			foreach (var stack in runedMetalStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalMetal += stackComp.Count;
			}
			foreach (var stack in runedGlassStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalGlass += stackComp.Count;
			}

			if (totalMetal < JuggernautMetalRequired && totalGlass < PylonGlassRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-insufficient-materials"),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalMetal < JuggernautMetalRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-metal", ("needed", JuggernautMetalRequired), ("have", totalMetal)),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalGlass < PylonGlassRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-glass", ("needed", PylonGlassRequired), ("have", totalGlass)),
					user, user, PopupType.MediumCaution
				);
			}
		}
	}

	/// <summary>
	/// Checks if enough materials exist without consuming them.
	/// Returns true if enough materials are available, false otherwise.
	/// </summary>
	private bool HasEnoughMaterials(List<EntityUid> stacks, int requiredAmount)
	{
		int totalCount = 0;
		foreach (var stackUid in stacks)
		{
			if (TryComp<StackComponent>(stackUid, out var stackComp))
				totalCount += stackComp.Count;
		}

		return totalCount >= requiredAmount;
	}

	/// <summary>
	/// Consumes the required amount from stacks, removing from multiple stacks if needed.
	/// Returns true if enough materials were consumed, false otherwise.
	/// </summary>
	private bool TryConsumeMaterials(List<EntityUid> stacks, int requiredAmount, EntityUid? user = null)
	{
		// First, sum up total count
		int totalCount = 0;
		foreach (var stackUid in stacks)
		{
			if (TryComp<StackComponent>(stackUid, out var stackComp))
				totalCount += stackComp.Count;
		}

		// Check if we have enough
		if (totalCount < requiredAmount)
			return false;

		// Consume materials from stacks
		int remainingToConsume = requiredAmount;
		foreach (var stackUid in stacks)
		{
			if (remainingToConsume <= 0)
				break;

			if (!TryComp<StackComponent>(stackUid, out var stackComp))
				continue;

			int availableInStack = stackComp.Count;
			int toRemove = Math.Min(remainingToConsume, availableInStack);

			// Remove the amount from this stack
			_stackSystem.SetCount(stackUid, availableInStack - toRemove, stackComp);
			remainingToConsume -= toRemove;

			// StackSystem.SetCount will automatically delete the stack if count reaches 0
		}

		return remainingToConsume == 0;
	}

	/// <summary>
	/// Checks if the rune location is free of hard physics collision objects (players, mobs, etc).
	/// Since the rune will be replaced by the pylon, we only need to check for blocking dynamic entities.
	/// </summary>
	/// <param name="coordinates">The coordinates of the rune</param>
	/// <param name="runeEntity">The rune entity to exclude from checks</param>
	private bool IsRuneLocationFree(EntityCoordinates coordinates, EntityUid runeEntity)
	{
		// Check if coordinates are valid
		if (!coordinates.IsValid(EntityManager))
			return false;

		// Get all entities intersecting the rune's location
		// Use a small radius to check the tile
		var intersectingEntities = _lookup.GetEntitiesInRange(coordinates, 0.5f, LookupFlags.Dynamic | LookupFlags.Static);

		foreach (var entity in intersectingEntities)
		{
			// Exclude the rune itself - it will be replaced
			if (entity == runeEntity)
				continue;

			// Allow puddles/liquids - they can exist under structures and don't block anchoring
			// Check this FIRST before physics checks, as puddles might not have hard physics bodies
			if (HasComp<PuddleComponent>(entity))
				continue;

			// Allow subfloor items - they don't block (cables, pipes, etc.)
			// Check this before physics checks
			if (HasComp<SubFloorHideComponent>(entity))
				continue;

			// Check if entity has a physics component
			if (!_physicsQuery.TryGetComponent(entity, out var body))
				continue;

			// Only block entities with hard, colliding physics bodies
			// This will catch players, mobs, and other blocking structures
			if (!body.CanCollide || !body.Hard)
				continue;

			// Check if it's a dynamic entity (player, mob) or an anchored structure
			var transform = Transform(entity);
			if (transform.Anchored)
			{
				// Block anchored hard structures (walls, other pylons, etc.)
				// Puddles and subfloor items already handled above
				return false;
			}
			else
			{
				// Block dynamic hard entities (players, mobs, etc.)
				return false;
			}
		}

		// Location is free
		return true;
	}
}

