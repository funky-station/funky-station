// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Fluids.EntitySystems;
using Content.Server.GameTicking.Rules;
using Content.Server.Medical;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntitySystems
{
	/// <summary>
	/// Handles blood consumption by cultists through the reactive system.
	/// When cultists ingest blood, they vomit Sanguine Perniculate and add to the ritual pool.
	/// This prevents double-counting (can't drink blood AND the resulting Sanguine Perniculate).
	/// 
	/// Performance: AddBloodForConversion is just a simple += operation on a singleton component,
	/// so no batching is needed - we update the counter immediately.
	/// </summary>
	public sealed class BloodCultBloodConsumptionSystem : EntitySystem
	{
		[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
		[Dependency] private readonly PuddleSystem _puddle = default!;
		[Dependency] private readonly VomitSystem _vomit = default!;

		public override void Initialize()
		{
			base.Initialize();

			// Subscribe to ingestion reactions to detect when blood is consumed
			SubscribeLocalEvent<BloodCultistComponent, ReactionEntityEvent>(OnBloodCultistReaction);
		}

	private void OnBloodCultistReaction(EntityUid uid, BloodCultistComponent component, ref ReactionEntityEvent args)
	{
		// Only process ingestion (drinking/eating)
		if (args.Method != ReactionMethod.Ingestion)
			return;

		// Only process sacrifice blood reagents (not Sanguine Perniculate)
		if (!BloodCultConstants.SacrificeBloodReagents.Contains(args.Reagent.ID))
			return;

		var bloodAmount = args.ReagentQuantity.Quantity;
		if (bloodAmount <= 0)
			return;

		// Add to the global ritual pool (this is just a += operation, very cheap)
		_bloodCultRule.AddBloodForConversion(bloodAmount.Float());

		// Make the cultist vomit Sanguine Perniculate
		var vomitSolution = new Solution();
		vomitSolution.AddReagent(new ReagentId("SanguinePerniculate", null), bloodAmount);
		_puddle.TrySpillAt(Transform(uid).Coordinates, vomitSolution, out _);

			// Trigger vomit effects (slowdown, popup)
			_vomit.Vomit(uid, thirstAdded: -bloodAmount.Float() / 2, hungerAdded: -bloodAmount.Float() / 2);
		}
	}
}

