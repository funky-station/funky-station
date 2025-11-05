// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.BloodCult.EntityEffects;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Robust.Shared.Timing;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// System that manages restoring original blood types after Edge Essentia wears off
/// and tracks Sanguine Perniculate being bled out for the ritual pool.
/// </summary>
public sealed class EdgeEssentiaBloodSystem : EntitySystem
{
	[Dependency] private readonly IGameTiming _timing = default!;
	[Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
	[Dependency] private readonly BloodstreamSystem _bloodstream = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;

	private TimeSpan _nextUpdate = TimeSpan.Zero;
	private bool _bloodCultRuleActive = false;

	public override void Update(float frameTime)
	{
		base.Update(frameTime);

		var curTime = _timing.CurTime;
		if (curTime < _nextUpdate)
			return;

		_nextUpdate = curTime + TimeSpan.FromSeconds(1);

		// Check if BloodCult game rule is active (cache this check)
		_bloodCultRuleActive = HasBloodCultRule();

		// Check all entities with EdgeEssentiaBloodComponent
		var query = EntityQueryEnumerator<EdgeEssentiaBloodComponent, BloodstreamComponent>();
		while (query.MoveNext(out var uid, out var edgeEssentia, out var bloodstream))
		{
			// Track how much Sanguine Perniculate they're bleeding out (only if cult rule is active)
			if (_bloodCultRuleActive)
			{
				TrackSanguinePerniculateLoss(uid, edgeEssentia, bloodstream);
			}

			// Check if they still have Edge Essentia in their system OR if their blood type is still SanguinePerniculate
			// Keep tracking as long as they're bleeding cursed blood
			if (HasEdgeEssentia(uid, bloodstream) || bloodstream.BloodReagent == "SanguinePerniculate")
				continue;

			// No Edge Essentia left AND blood type has been restored, remove the component
			_bloodstream.ChangeBloodReagent(uid, edgeEssentia.OriginalBloodReagent, bloodstream);
			RemCompDeferred<EdgeEssentiaBloodComponent>(uid);
		}
	}

	/// <summary>
	/// Checks if the BloodCult game rule is currently active
	/// </summary>
	private bool HasBloodCultRule()
	{
		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		return query.MoveNext(out _, out _, out _);
	}

	private void TrackSanguinePerniculateLoss(EntityUid uid, EdgeEssentiaBloodComponent edgeEssentia, BloodstreamComponent bloodstream)
	{
		// Only track if they're actively bleeding
		if (bloodstream.BleedAmount <= 0)
			return;

		// Only count blood from player-controlled entities (those with an ACTUAL mind, not just the component)
		// This prevents farming non-sentient entities like slimes, animals, etc.
		if (!TryComp<MindContainerComponent>(uid, out var mindContainer) || mindContainer.Mind == null)
			return;

		// Check if this entity has reached the blood collection cap
		var tracker = EnsureComp<BloodCollectionTrackerComponent>(uid);
		if (tracker.TotalBloodCollected >= tracker.MaxBloodPerEntity)
			return;

		// Check the temporary blood solution to see how much Sanguine Perniculate was bled out
		if (!_solutionContainer.ResolveSolution(uid, bloodstream.BloodTemporarySolutionName, ref bloodstream.TemporarySolution, out var tempSolution))
			return;

		// Look for Sanguine Perniculate in the temporary solution (blood that's being/about to be spilled)
		FixedPoint2 sanguineAmount = FixedPoint2.Zero;
		foreach (var reagent in tempSolution.Contents)
		{
			if (reagent.Reagent.Prototype == "SanguinePerniculate")
			{
				sanguineAmount += reagent.Quantity;
			}
		}

		// If Sanguine Perniculate was bled out and we haven't tracked it yet
		if (sanguineAmount > 0 && sanguineAmount > edgeEssentia.LastTrackedBloodAmount)
		{
			var newBloodLoss = sanguineAmount - edgeEssentia.LastTrackedBloodAmount;
			
			// Enforce the per-entity cap
			var remainingCapacity = tracker.MaxBloodPerEntity - tracker.TotalBloodCollected;
			var bloodToAdd = Math.Min(newBloodLoss.Float(), remainingCapacity);
			
			if (bloodToAdd > 0)
			{
				// Add to the ritual pool
				_bloodCultRule.AddBloodForConversion(bloodToAdd);
				
				// Update the tracker
				tracker.TotalBloodCollected += bloodToAdd;
			}
			
			// Update the tracked amount
			edgeEssentia.LastTrackedBloodAmount = sanguineAmount;
		}
		// If the temporary solution was cleared (blood was spilled), reset the tracker
		else if (sanguineAmount == 0)
		{
			edgeEssentia.LastTrackedBloodAmount = FixedPoint2.Zero;
		}
	}

	private bool HasEdgeEssentia(EntityUid uid, BloodstreamComponent bloodstream)
	{
		// Check if there's Edge Essentia in their chemical solution (metabolism container)
		if (!_solutionContainer.ResolveSolution(uid, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution))
			return false;

		foreach (var reagent in chemSolution.Contents)
		{
			if (reagent.Reagent.Prototype == "EdgeEssentia")
				return true;
		}

		return false;
	}
}

