// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System;
using System.Numerics;
using Content.Server.Stack;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Trigger;
using Content.Shared.Stacks;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.SubFloor;
using Content.Shared.Fluids.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
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
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	//[Dependency] private readonly IPrototypeManager _protoMan = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;

	private EntityQuery<PhysicsComponent> _physicsQuery = default!;

	private const int JuggernautMetalRequired = 30;
	private const int PylonGlassRequired = 10;
	private const int ForsakenBootsPlasticRequired = 5;
	private const int ForsakenBootsClothRequired = 5;
	private const int ForsakenBootsDurathreadRequired = 5;
	private const int AcolyteArmorPlasteelRequired = 10;

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
		List<EntityUid> runedSteelStacks = new List<EntityUid>();
		List<EntityUid> runedGlassStacks = new List<EntityUid>();
		List<EntityUid> plasticStacks = new List<EntityUid>();
		List<EntityUid> clothStacks = new List<EntityUid>();
		List<EntityUid> durathreadStacks = new List<EntityUid>();
		List<EntityUid> runedPlasteelStacks = new List<EntityUid>();

		foreach (var entity in summonLookup)
		{
			if (!TryComp<StackComponent>(entity, out var stack))
				continue;

			if (stack.StackTypeId == "RunedSteel")
				runedSteelStacks.Add(entity);
			else if (stack.StackTypeId == "RunedGlass")
				runedGlassStacks.Add(entity);
			else if (stack.StackTypeId == "Plastic")
				plasticStacks.Add(entity);
			else if (stack.StackTypeId == "Cloth")
				clothStacks.Add(entity);
			else if (stack.StackTypeId == "Durathread")
				durathreadStacks.Add(entity);
			else if (stack.StackTypeId == "RunedPlasteel")
				runedPlasteelStacks.Add(entity);
		}

		// Check for 30 runed steel - spawn Juggernaut shell
		// First check if enough materials exist (without consuming)
		if (HasEnoughMaterials(runedSteelStacks, JuggernautMetalRequired))
		{
			// Only consume materials AFTER validation passes
			if (TryConsumeMaterials(runedSteelStacks, JuggernautMetalRequired, user))
			{
				var juggernautShell = Spawn("CultJuggernautShell", runeCoords);
				
				// Ensure the shell is not anchored (it should be movable)
				var shellTransform = Transform(juggernautShell);
				if (shellTransform.Anchored)
				{
					_transform.Unanchor(juggernautShell, shellTransform);
				}
				
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-juggernaut-shell"),
					user, user, PopupType.Large
				);
				
				// Delete the rune after successful summoning
				QueueDel(uid);
				args.Handled = true;
				return;
			}
		}

		// Check for 10 runedplasteel + outerwear item - spawn Acolyte Armor and Cult Helmet
		// First check if enough plasteel materials exist (without consuming)
		if (HasEnoughMaterials(runedPlasteelStacks, AcolyteArmorPlasteelRequired))
		{
			// Find outerwear items in range (similar to cosmic cult transmute logic)
			EntityUid? outerwearItem = null;
			foreach (var entity in summonLookup)
			{
				// Check if it's an outerwear item (has ClothingComponent with OUTERCLOTHING slot flag)
				// summonLookup already excludes items in containers via LookupFlags.Uncontained
				if (TryComp<ClothingComponent>(entity, out var clothing) && 
				    clothing.Slots.HasFlag(SlotFlags.OUTERCLOTHING))
				{
					// Found a valid outerwear item
					outerwearItem = entity;
					break;
				}
			}

			// Need both plasteel and outerwear item
			if (outerwearItem != null && Exists(outerwearItem.Value))
			{
				// Only consume materials AFTER validation passes
				if (TryConsumeMaterials(runedPlasteelStacks, AcolyteArmorPlasteelRequired, user))
				{
					// Delete the outerwear item (it's being transformed into acolyte armor)
					QueueDel(outerwearItem.Value);

				// Spawn acolyte armor at the rune coordinates
				var acolyteArmor = Spawn("ClothingOuterArmorCult", runeCoords);
				var cultHelmet = Spawn("ClothingHeadHelmetCult", runeCoords);
					
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-acolyte-armor"),
						user, user, PopupType.Large
					);
					
					// Delete the rune after successful summoning
					QueueDel(uid);
					args.Handled = true;
					return;
				}
			}
			else
			{
				// We have enough plasteel but no outerwear item found
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-outerwear"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}
		}

		// Check for Forsaken Boots - can use either 5 plastic + 5 cloth OR 5 durathread
		// First check if enough materials exist (without consuming)
		if (HasEnoughMaterials(plasticStacks, ForsakenBootsPlasticRequired) && 
		    HasEnoughMaterials(clothStacks, ForsakenBootsClothRequired))
		{
			// Primary method: 5 plastic + 5 cloth
			if (TryConsumeMaterials(plasticStacks, ForsakenBootsPlasticRequired, user) &&
			    TryConsumeMaterials(clothStacks, ForsakenBootsClothRequired, user))
			{
				var forsakenBoots = Spawn("ClothingShoesBootsForsaken", runeCoords);
				
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-forsaken-boots"),
					user, user, PopupType.Large
				);
				
				// Delete the rune after successful summoning
				QueueDel(uid);
				args.Handled = true;
				return;
			}
		}
		else if (HasEnoughMaterials(durathreadStacks, ForsakenBootsDurathreadRequired) &&
		         !HasEnoughMaterials(plasticStacks, ForsakenBootsPlasticRequired) &&
		         !HasEnoughMaterials(clothStacks, ForsakenBootsClothRequired))
		{
			// Alternative method: 5 durathread (only if no plastic/cloth available)
			if (TryConsumeMaterials(durathreadStacks, ForsakenBootsDurathreadRequired, user))
			{
				var forsakenBoots = Spawn("ClothingShoesBootsForsaken", runeCoords);
				
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-forsaken-boots"),
					user, user, PopupType.Large
				);
				
				// Delete the rune after successful summoning
				QueueDel(uid);
				args.Handled = true;
				return;
			}
		}
		else if (HasEnoughMaterials(plasticStacks, ForsakenBootsPlasticRequired))
		{
			// We have enough plastic but not enough cloth - calculate cloth count
			int clothCount = GetTotalStackCount(clothStacks);
			_popupSystem.PopupEntity(
				Loc.GetString("cult-summoning-need-more-cloth", ("needed", ForsakenBootsClothRequired), ("have", clothCount)),
				user, user, PopupType.MediumCaution
			);
			args.Handled = true;
			return;
		}
		else if (HasEnoughMaterials(clothStacks, ForsakenBootsClothRequired))
		{
			// We have enough cloth but not enough plastic - calculate plastic count
			int plasticCount = GetTotalStackCount(plasticStacks);
			_popupSystem.PopupEntity(
				Loc.GetString("cult-summoning-need-more-plastic", ("needed", ForsakenBootsPlasticRequired), ("have", plasticCount)),
				user, user, PopupType.MediumCaution
			);
			args.Handled = true;
			return;
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

			// Second check: Verify location is free of anchored structures that would prevent anchoring
			// We only check for anchored structures - dynamic entities (players, mobs) won't prevent anchoring
			if (!IsRuneLocationFreeForAnchoring(runeCoords, uid))
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

				// Verify pylon exists
				if (!Exists(pylon.Value))
				{
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Get transform and set the rotation to match the rune
				var pylonXform = Transform(pylon.Value);
				pylonXform.LocalRotation = runeRotation;

				// Verify pylon is actually anchored (it should be since CultPylon has anchored: true)
				// Refresh transform to get latest state
				if (!pylonXform.Anchored || !Exists(pylon.Value))
				{
					// Pylon didn't anchor - try spawning unanchored nearby as fallback
					if (Exists(pylon.Value))
						QueueDel(pylon.Value);
					
					// Try to find a nearby location to spawn unanchored pylon
					if (TryFindNearbyLocation(runeCoordsForPylon, out var nearbyLocation))
					{
						var unanchoredPylon = Spawn("CultPylon", nearbyLocation);
						if (Exists(unanchoredPylon))
						{
							var unanchoredXform = Transform(unanchoredPylon);
							// Ensure it's unanchored
							_transform.Unanchor(unanchoredPylon, unanchoredXform);
							unanchoredXform.LocalRotation = runeRotation;
							
							// Consume materials for unanchored pylon
							if (TryConsumeMaterials(runedGlassStacks, PylonGlassRequired, user))
							{
								// Play the same activation sound that other cult runes use
								_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Effects/inneranomaly.ogg"), nearbyLocation);
								_popupSystem.PopupEntity(
									Loc.GetString("cult-summoning-pylon"),
									user, user, PopupType.Large
								);
								args.Handled = true;
								return;
							}
							else
							{
								QueueDel(unanchoredPylon);
							}
						}
					}
					
					// Fallback failed - show error
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-anchor-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}

				// Double-check pylon still exists after a moment (in case something deleted it)
				if (!Exists(pylon.Value))
				{
					_popupSystem.PopupEntity(
						Loc.GetString("cult-summoning-pylon-failed"),
						user, user, PopupType.MediumCaution
					);
					args.Handled = true;
					return;
				}
				
				var pylonXformCheck = Transform(pylon.Value);
				if (!pylonXformCheck.Anchored)
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
				// Play the same activation sound that other cult runes use
				_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Effects/inneranomaly.ogg"), runeCoordsForPylon);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-pylon"),
					user, user, PopupType.Large
				);
				args.Handled = true;
				return;
			}
			catch (Exception)
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

		// No valid materials found
		if (runedSteelStacks.Count == 0 && runedGlassStacks.Count == 0 && plasticStacks.Count == 0 && clothStacks.Count == 0 && durathreadStacks.Count == 0 && runedPlasteelStacks.Count == 0)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-summoning-no-materials"),
				user, user, PopupType.MediumCaution
			);
		}
		else
		{
			// Calculate what materials are present and what's missing
			int totalSteel = 0;
			int totalGlass = 0;
			int totalPlastic = 0;
			int totalCloth = 0;
			int totalDurathread = 0;
			int totalPlasteel = 0;
			foreach (var stack in runedSteelStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalSteel += stackComp.Count;
			}
			foreach (var stack in runedGlassStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalGlass += stackComp.Count;
			}
			foreach (var stack in plasticStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalPlastic += stackComp.Count;
			}
			foreach (var stack in clothStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalCloth += stackComp.Count;
			}
			foreach (var stack in durathreadStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalDurathread += stackComp.Count;
			}
			foreach (var stack in runedPlasteelStacks)
			{
				if (TryComp<StackComponent>(stack, out var stackComp))
					totalPlasteel += stackComp.Count;
			}

			if (totalSteel < JuggernautMetalRequired && totalGlass < PylonGlassRequired && totalPlastic < ForsakenBootsPlasticRequired && totalCloth < ForsakenBootsClothRequired && totalDurathread < ForsakenBootsDurathreadRequired && totalPlasteel < AcolyteArmorPlasteelRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-insufficient-materials"),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalSteel < JuggernautMetalRequired && totalGlass < PylonGlassRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-plastic", ("needed", ForsakenBootsPlasticRequired), ("have", totalPlastic)),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalSteel < JuggernautMetalRequired && totalPlastic < ForsakenBootsPlasticRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-glass", ("needed", PylonGlassRequired), ("have", totalGlass)),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalGlass < PylonGlassRequired && totalPlastic < ForsakenBootsPlasticRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-steel", ("needed", JuggernautMetalRequired), ("have", totalSteel)),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalSteel < JuggernautMetalRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-steel", ("needed", JuggernautMetalRequired), ("have", totalSteel)),
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
			else if (totalPlastic < ForsakenBootsPlasticRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-plastic", ("needed", ForsakenBootsPlasticRequired), ("have", totalPlastic)),
					user, user, PopupType.MediumCaution
				);
			}
			else if (totalCloth < ForsakenBootsClothRequired)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-summoning-need-more-cloth", ("needed", ForsakenBootsClothRequired), ("have", totalCloth)),
					user, user, PopupType.MediumCaution
				);
			}
		}
	}

	/// <summary>
	/// Gets the total count from a list of stack entities.
	/// </summary>
	private int GetTotalStackCount(List<EntityUid> stacks)
	{
		int totalCount = 0;
		foreach (var stackUid in stacks)
		{
			if (TryComp<StackComponent>(stackUid, out var stackComp))
				totalCount += stackComp.Count;
		}
		return totalCount;
	}

	/// <summary>
	/// Checks if enough materials exist without consuming them.
	/// Returns true if enough materials are available, false otherwise.
	/// </summary>
	private bool HasEnoughMaterials(List<EntityUid> stacks, int requiredAmount)
	{
		return GetTotalStackCount(stacks) >= requiredAmount;
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
	/// Checks if the rune location is free of anchored structures that would prevent anchoring.
	/// Only checks for anchored structures - dynamic entities (players, mobs) won't prevent anchoring.
	/// </summary>
	/// <param name="coordinates">The coordinates of the rune</param>
	/// <param name="runeEntity">The rune entity to exclude from checks</param>
	private bool IsRuneLocationFreeForAnchoring(EntityCoordinates coordinates, EntityUid runeEntity)
	{
		// Check if coordinates are valid
		if (!coordinates.IsValid(EntityManager))
			return false;

		// Get the grid and tile indices
		var gridUid = _transform.GetGrid(coordinates);
		if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
			return false;

		var tileIndices = _mapSystem.TileIndicesFor(gridUid.Value, grid, coordinates);

		// Check for anchored entities on this tile that would block anchoring
		foreach (var anchoredEntity in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, tileIndices))
		{
			// Exclude the rune itself - it will be replaced
			if (anchoredEntity == runeEntity)
				continue;

			// Allow puddles/liquids - they can exist under structures and don't block anchoring
			if (HasComp<PuddleComponent>(anchoredEntity))
				continue;

			// Allow subfloor items - they don't block (cables, pipes, etc.)
			if (HasComp<SubFloorHideComponent>(anchoredEntity))
				continue;

			// Check if entity has a physics component
			if (!_physicsQuery.TryGetComponent(anchoredEntity, out var body))
				continue;

			// Only block anchored hard structures that would prevent anchoring
			// Dynamic entities (players, mobs) are not anchored, so they won't block
			if (body.CanCollide && body.Hard)
			{
				// This anchored structure would block anchoring
				return false;
			}
		}

		// Location is free for anchoring
		return true;
	}

	/// <summary>
	/// Tries to find a nearby location to spawn an unanchored pylon.
	/// Checks adjacent tiles in a spiral pattern.
	/// </summary>
	/// <param name="centerCoords">The center coordinates to search around</param>
	/// <param name="nearbyLocation">The found nearby location, or invalid if none found</param>
	/// <returns>True if a nearby location was found, false otherwise</returns>
	private bool TryFindNearbyLocation(EntityCoordinates centerCoords, out EntityCoordinates nearbyLocation)
	{
		nearbyLocation = EntityCoordinates.Invalid;

		// Check if center coordinates are valid
		if (!centerCoords.IsValid(EntityManager))
			return false;

		// Get the grid
		var gridUid = _transform.GetGrid(centerCoords);
		if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
			return false;

		// Search in a spiral pattern around the center (up to 2 tiles away)
		var offsets = new[]
		{
			new Vector2(0f, 1.2f),   // North
			new Vector2(1.2f, 0f),   // East
			new Vector2(0f, -1.2f),  // South
			new Vector2(-1.2f, 0f),  // West
			new Vector2(1.2f, 1.2f), // Northeast
			new Vector2(1.2f, -1.2f), // Southeast
			new Vector2(-1.2f, -1.2f), // Southwest
			new Vector2(-1.2f, 1.2f),  // Northwest
		};

		foreach (var offset in offsets)
		{
			var candidateCoords = centerCoords.Offset(offset);
			var candidateLocation = candidateCoords.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapManager);

			// Check if this location is free (no anchored blocking structures)
			// Use EntityUid.Invalid as the rune entity since we're checking a different location
			if (IsRuneLocationFreeForAnchoring(candidateLocation, EntityUid.Invalid))
			{
				nearbyLocation = candidateLocation;
				return true;
			}
		}

		// No nearby location found
		return false;
	}
}

