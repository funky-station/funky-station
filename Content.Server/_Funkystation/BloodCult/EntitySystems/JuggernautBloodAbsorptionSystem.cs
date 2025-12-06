// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Fluids.EntitySystems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// System that handles juggernauts absorbing blood from puddles beneath them to heal.
/// </summary>
public sealed class JuggernautBloodAbsorptionSystem : EntitySystem
{
	[Dependency] private readonly DamageableSystem _damageable = default!;
	[Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly EntityLookupSystem _lookup = default!;
	[Dependency] private readonly IGameTiming _timing = default!;
	// Use these controls to adjust the rate that a juggernaut can self heal.
	private const float AbsorptionRate = 2.0f; // Units per second
	private const float UpdateInterval = 0.5f; // Check every 0.5 seconds
	private const float MinDamageThreshold = 5.0f; // Must have more than 5 damage to absorb. Heals up most of the way without over-healing.

	private TimeSpan _lastUpdate;

	public override void Initialize()
	{
		base.Initialize();
		_lastUpdate = _timing.CurTime;
	}

	public override void Update(float frameTime)
	{
		base.Update(frameTime);

		var curTime = _timing.CurTime;
		if (curTime - _lastUpdate < TimeSpan.FromSeconds(UpdateInterval))
			return;

		_lastUpdate = curTime;

		var query = EntityQueryEnumerator<JuggernautComponent, DamageableComponent>();
		while (query.MoveNext(out var uid, out var juggernaut, out var damageable))
		{
			// Skip if juggernaut is dead (can't heal dead juggernauts)
			if (TryComp<MobStateComponent>(uid, out var mobState) && mobState.CurrentState == MobState.Dead)
				continue;

			// Check if juggernaut has more than the threshold damage
			var totalDamage = damageable.Damage.GetTotal();
			if (totalDamage <= MinDamageThreshold)
				continue;

		// Check if juggernaut contains a soulstone or dead body (player inside).
		// Only heal if there's a player entity in the juggernaut.
		// Check the component fields directly - these are set to null when ejected, so they're reliable.
		bool hasPlayer = false;
		if (juggernaut.SourceSoulstone != null && Exists(juggernaut.SourceSoulstone.Value))
			hasPlayer = true;
		else if (juggernaut.SourceBody != null && Exists(juggernaut.SourceBody.Value))
			hasPlayer = true;
		
		// Skip healing if no player entity is present
		if (!hasPlayer)
			continue;

			// Get the puddle at the juggernaut's position
			// Use a small range to find puddles near the juggernaut (0.5 units should cover the tile)
			var transform = Transform(uid);
			var coordinates = transform.Coordinates;
			var puddlesInRange = _lookup.GetEntitiesInRange<PuddleComponent>(coordinates, 0.5f, LookupFlags.Uncontained);
			
			if (puddlesInRange.Count == 0)
				continue;
			
			// Find the closest puddle (or just use the first one if multiple)
			// This handles if a juggernaut is partway between two tiles and it just picks the closest
			EntityUid? puddleUid = null;
			var juggernautMapPos = _transform.GetMapCoordinates(uid, transform);
			float closestDistance = float.MaxValue;
			
			foreach (var puddleEntity in puddlesInRange)
			{
				var puddleMapPos = _transform.GetMapCoordinates(puddleEntity);
				var distance = (puddleMapPos.Position - juggernautMapPos.Position).Length();
				if (distance < closestDistance)
				{
					closestDistance = distance;
					puddleUid = puddleEntity;
				}
			}
			
			if (puddleUid == null)
				continue;

			// Get the puddle's solution
			if (!TryComp<PuddleComponent>(puddleUid, out var puddleComp))
				continue;

			// Safety check on puddle solution
			if (!_solutionContainer.ResolveSolution((puddleUid.Value, null), puddleComp.SolutionName, ref puddleComp.Solution, out var solution))
				continue;

			// Safety check on puddle volume
			if (solution.Volume <= FixedPoint2.Zero)
				continue;

			// Check if the solution contains valid blood reagents. This is the list of reagents that are blood, plus Sanguine Perniculate specifically.
			var validReagent = FindValidBloodReagent(solution);
			if (validReagent == null)
				continue;

			// Calculate how much to absorb, based on the absorption rate and update interval.
			var absorbAmount = FixedPoint2.New(AbsorptionRate * UpdateInterval);
			var reagentId = new ReagentId(validReagent, null);
			if (absorbAmount > solution.GetReagentQuantity(reagentId))
				absorbAmount = solution.GetReagentQuantity(reagentId);

			if (absorbAmount <= FixedPoint2.Zero)
				continue;

			// Remove the reagent from the puddle using the solution container system
			if (puddleComp.Solution != null)
			{
				_solutionContainer.RemoveReagent(puddleComp.Solution.Value, reagentId, absorbAmount);
			}

			// Heal the juggernaut 1:1 (1 unit blood = 1 unit total healing)
			// Heal all damage types proportionally based on current damage
			var currentDamage = damageable.Damage;
			var totalCurrentDamage = currentDamage.GetTotal();
			var healAmount = absorbAmount.Float();
			var healDamage = new DamageSpecifier();

			// Maybe too complicated, but this ensures that blood always heals it
			if (totalCurrentDamage > 0)
			{
				// Heal each damage type proportionally
				foreach (var kvp in currentDamage.DamageDict)
				{
					var damageType = kvp.Key;
					var amount = kvp.Value;
					if (amount > 0)
					{
						var proportion = amount.Float() / totalCurrentDamage.Float();
						healDamage.DamageDict[damageType] = -healAmount * proportion;
					}
				}
			}
			else
			{
				// If no damage, heal Brute and Burn equally as fallback
				healDamage.DamageDict["Brute"] = -healAmount / 2;
				healDamage.DamageDict["Burn"] = -healAmount / 2;
			}

			_damageable.TryChangeDamage(uid, healDamage, true);
		}
	}

	/// <summary>
	/// Finds a valid blood reagent in the solution. Returns null if none found.
	/// Valid reagents are those in SacrificeBloodReagents or SanguinePerniculate.
	/// Prioritizes non-SanguinePerniculate blood types first, then falls back to SanguinePerniculate.
	/// </summary>
	private string? FindValidBloodReagent(Solution solution)
	{
		// Check for any blood reagent in the whitelist first (prioritize regular blood over SanguinePerniculate)
		foreach (var bloodReagent in BloodCultConstants.SacrificeBloodReagents)
		{
			var reagentId = new ReagentId(bloodReagent, null);
			if (solution.GetReagentQuantity(reagentId) > FixedPoint2.Zero)
				return bloodReagent;
		}

		// Fall back to SanguinePerniculate if no other blood types are found
		var sanguineId = new ReagentId("SanguinePerniculate", null);
		if (solution.GetReagentQuantity(sanguineId) > FixedPoint2.Zero)
			return "SanguinePerniculate";

		return null;
	}
}

