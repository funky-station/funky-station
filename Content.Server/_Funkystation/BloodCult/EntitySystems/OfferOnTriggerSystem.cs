// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using System.Linq;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Content.Shared.FixedPoint;
using Content.Shared.Trigger;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.BloodCult;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mindshield.Components;
using Content.Shared.Body.Part;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Roles;
using Content.Server.Roles;
using Content.Server.Mind;
using Content.Shared.Stunnable;
using Content.Shared.StatusEffect;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GameTicking.Rules;

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class OfferOnTriggerSystem : EntitySystem
	{
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly EntityLookupSystem _lookup = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
		[Dependency] private readonly SharedRoleSystem _role = default!;
		[Dependency] private readonly BloodCultistSystem _bloodCultist = default!;
		[Dependency] private readonly MindSystem _mind = default!;
		[Dependency] private readonly SharedAudioSystem _audio = default!;
		[Dependency] private readonly BloodCultConstructSystem _constructSystem = default!;
		[Dependency] private readonly SharedContainerSystem _container = default!;
		[Dependency] private readonly BloodstreamSystem _bloodstream = default!;
		[Dependency] private readonly SharedStunSystem _stun = default!;
		[Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
		[Dependency] private readonly PuddleSystem _puddle = default!;
		[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;

		public override void Initialize()
		{
			base.Initialize();
			SubscribeLocalEvent<OfferOnTriggerComponent, TriggerEvent>(HandleOfferTrigger);
		}

		private void HandleOfferTrigger(EntityUid uid, OfferOnTriggerComponent component, TriggerEvent args)
		{
			if (args.User == null)
				return;
			EntityUid user = (EntityUid)args.User;

			if (!TryComp(user, out BloodCultistComponent? bloodCultist))
				return;

			var offerLookup = _lookup.GetEntitiesInRange(uid, component.OfferRange);
			var invokeLookup = _lookup.GetEntitiesInRange(uid, component.InvokeRange);
			EntityUid[] cultistsInRange = Array.FindAll(invokeLookup.ToArray(), item => ((HasComp<BloodCultistComponent>(item) || HasComp<BloodCultConstructComponent>(item)) && !_mobState.IsDead(item)));

			List<EntityUid> humanoids = new List<EntityUid>();
			List<EntityUid> brains = new List<EntityUid>();
			List<EntityUid> shells = new List<EntityUid>();
			foreach (var look in offerLookup)
			{
				if (HasComp<HumanoidAppearanceComponent>(look))
					humanoids.Add(look);
				else if (HasComp<BrainComponent>(look))
					brains.Add(look);
				else if (HasComp<BloodCultConstructShellComponent>(look))
					shells.Add(look);
			}

			EntityUid? candidate = null;
			if (humanoids.Count > 0)
				candidate = humanoids[0];
			else if (brains.Count > 0)
				candidate = brains[0];

			if (candidate != null)
			{
				EntityUid offerable = (EntityUid) candidate;

				if (!_IsValidTarget(offerable, out var mind))
				{
					_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-fail-nosoul"),
							user, user, PopupType.MediumCaution
						);
				}
			else if (HasComp<BloodCultistComponent>(offerable) || (mind != null && _role.MindHasRole<BloodCultRoleComponent>((EntityUid)mind)))
			{
				_popupSystem.PopupEntity(
						Loc.GetString("cult-invocation-fail-teamkill"),
						user, user, PopupType.MediumCaution
					);
			}
			else if (_CanBeConverted(offerable))
			{
				// Check if this is a soulstone-eligible entity (IPC, brain types, etc.)
				if (_IsSoulstoneEligible(offerable))
				{
					// Create a soulstone for special entities
					_CreateSoulstoneFromEntity(offerable, user, uid, cultistsInRange);
				}
				else
				{
					// Normal conversion for organic humanoids
					_bloodCultist.UseConvertRune(offerable, user, uid, cultistsInRange);
					// Add 100u of blood to the ritual pool for converting someone
					_bloodCultRule.AddBloodForConversion(100.0);
				}
			}
			else if (HasComp<MindShieldComponent>(offerable) && shells.Count > 0)
			{
				// Mindshielded victim with a shell present - attempt to break mindshield
				_BreakMindshield(offerable, user, shells[0], cultistsInRange);
			}
			else if (_CanBeSacrificed(offerable, shells))
			{
				// Dead or otherwise non-convertible entity with shell - sacrifice into shell
				_SacrificeIntoShell(offerable, user, shells[0], cultistsInRange);
			}
			else if (shells.Count == 0)
			{
				// No shell present - show error message
				_popupSystem.PopupEntity(
						Loc.GetString("cult-invocation-fail-noshell"),
						user, user, PopupType.MediumCaution
					);
			}
			else
			{
				_popupSystem.PopupEntity(
						Loc.GetString("cult-invocation-fail-mindshielded"),
						user, user, PopupType.MediumCaution
					);
			}
			}
			else
			{
				// No valid conversion or sacrifice target found, check for blood containers
				_ProcessBloodContainers(uid, user, offerLookup);
			}
			args.Handled = true;
		}

		private void _ProcessBloodContainers(EntityUid rune, EntityUid user, HashSet<EntityUid> entitiesOnRune)
		{
			bool processedAny = false;

			foreach (var entity in entitiesOnRune)
			{
				// Skip if it's a humanoid or brain (already handled above)
				if (HasComp<HumanoidAppearanceComponent>(entity) || HasComp<BrainComponent>(entity))
					continue;

			// Try to get solution container - we only check direct solutions, not nested containers
			if (!_solutionContainer.TryGetSolution(entity, "beaker", out var beakerSolution, out var solution) &&
				!_solutionContainer.TryGetSolution(entity, "drink", out beakerSolution, out solution))
				continue;
			
			// Check if the solution contains any blood types
			var bloodTypes = new[] { "Blood", "Slime", "Sap", "InsectBlood", "CopperBlood", "AmmoniaBlood", "ZombieBlood", "ChangelingBlood" };
			FixedPoint2 totalBloodAmount = FixedPoint2.Zero;
			var bloodReagentsFound = new List<(ReagentId reagent, FixedPoint2 amount)>();

			foreach (var reagentQuantity in solution.Contents)
			{
				if (bloodTypes.Contains(reagentQuantity.Reagent.Prototype))
				{
					bloodReagentsFound.Add((reagentQuantity.Reagent, reagentQuantity.Quantity));
					totalBloodAmount += reagentQuantity.Quantity;
				}
			}

			if (totalBloodAmount <= 0)
				continue;

			// Convert all blood types to UnholyBlood
			foreach (var (reagent, amount) in bloodReagentsFound)
			{
				solution.RemoveReagent(reagent, amount);
			}
			solution.AddReagent(new ReagentId("UnholyBlood", null), totalBloodAmount);			_solutionContainer.UpdateChemicals(beakerSolution.Value);

			// Spill the container
			var spillSolution = _solutionContainer.SplitSolution(beakerSolution.Value, solution.Volume);
			_puddle.TrySpillAt(Transform(entity).Coordinates, spillSolution, out _);

				// Play audio and show popup
				_audio.PlayPvs(new SoundPathSpecifier("/Audio/_Funkystation/Ambience/Antag/creepyshriek.ogg"), Transform(rune).Coordinates);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-blood-transmuted"),
					user, user, PopupType.Medium
				);

				processedAny = true;
			}

			if (!processedAny)
			{
				// No valid target or container found
				_popupSystem.PopupEntity(
					Loc.GetString("cult-invocation-fail"),
					user, user, PopupType.MediumCaution
				);
			}
		}

		private bool _IsSacrificeTarget(EntityUid target, BloodCultistComponent comp)
		{
			return comp.Targets.Contains(target);
		}

		private bool _IsValidTarget(EntityUid uid, out Entity<MindComponent>? mind)
		{
			mind = null;
			if (TryComp(uid, out MindContainerComponent? mindContainer) &&
				mindContainer.Mind != null &&
				TryComp((EntityUid)mindContainer.Mind, out MindComponent? mindComponent))
				mind = ((EntityUid)mindContainer.Mind, (MindComponent) mindComponent);
			return mind != null;  // must have a soul
		}

	private bool _CanBeConverted(EntityUid uid)
	{
		// Allow soulstone-eligible entities even if dead
		if (_IsSoulstoneEligible(uid))
			return !HasComp<MindShieldComponent>(uid); //It shouldn't reach here if they have a mindshield, but just for safety
		
		// Regular entities must be alive
		return !_mobState.IsDead(uid) &&  // must not be dead
			!HasComp<MindShieldComponent>(uid);  // must not be mindshielded
	}

	private bool _IsSoulstoneEligible(EntityUid uid)
	{
		// Check if it's an IPC (via species check)
		if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid) && humanoid.Species == "IPC")
			return true;

		// Check entity prototype ID for specific brain/core/slime types
		if (TryComp<MetaDataComponent>(uid, out var meta) && meta.EntityPrototype != null)
		{
			var protoId = meta.EntityPrototype.ID;
			var soulstoneEntities = new[] 
			{ 
				"PositronicBrain", 
				"MobDionaNymph",
				"OrganThavenBrain"
			};
			
			if (soulstoneEntities.Contains(protoId))
				return true;
			
			// Check for adult slimes (any color variant)
			if (protoId.StartsWith("MobAdultSlimes"))
				return true;
		}

		// Check for regular brain component (already in brains list, but covers all brain types)
		if (HasComp<BrainComponent>(uid))
			return true;

		return false;
	}

	private void _CreateSoulstoneFromEntity(EntityUid victim, EntityUid user, EntityUid rune, EntityUid[] cultistsInRange)
	{
		// Get the victim's mind
		EntityUid? mindId = CompOrNull<MindContainerComponent>(victim)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail-nosoul"),
				user, user, PopupType.MediumCaution
			);
			return;
		}

		// Check if enough cultists are present
		if (cultistsInRange.Length < 1)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				user, user, PopupType.MediumCaution
			);
			return;
		}

		var coordinates = Transform(victim).Coordinates;
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), coordinates);
		
		// Create soulstone and transfer mind
		var soulstone = Spawn("CultSoulStone", coordinates);
		_mind.TransferTo((EntityUid)mindId, soulstone, mind:mindComp);
		
		// Delete the original entity
		QueueDel(victim);
		
		// Play success audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);
		
		// Notify the cultists
		_popupSystem.PopupEntity(
			Loc.GetString("cult-soulstone-created"),
			user, user, PopupType.Large
		);
		
		// Add blood to the ritual pool
		_bloodCultRule.AddBloodForConversion(100.0);
	}

	private bool _CanBeSacrificed(EntityUid uid, List<EntityUid> shells)
	{
		// Sacrifice requires a juggernaut shell to be present
		return shells.Count > 0;
	}

	private void _BreakMindshield(EntityUid victim, EntityUid user, EntityUid shell, EntityUid[] cultistsInRange)
	{
		// Check if enough cultists are present
		if (cultistsInRange.Length < 1)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				user, user, PopupType.MediumCaution
			);
			return;
		}

		var shellCoordinates = Transform(shell).Coordinates;
		
		// Play dramatic audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/_Funkystation/Ambience/Antag/creepyshriek.ogg"), shellCoordinates);
		
		// Show dramatic message to all cultists
		foreach (EntityUid cultist in cultistsInRange)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-mindshield-break"),
				cultist, cultist, PopupType.LargeCaution
			);
		}
		
		// Apply bleeding and stun to all participating cultists
		foreach (EntityUid cultist in cultistsInRange)
		{
			// Apply heavy bleeding
			if (TryComp<BloodstreamComponent>(cultist, out var bloodstream))
			{
				_bloodstream.TryModifyBleedAmount(cultist, 5.0f, bloodstream);
			}
			
			// Apply stun and knockdown
			if (TryComp<StatusEffectsComponent>(cultist, out var status))
			{
				_stun.TryParalyze(cultist, TimeSpan.FromSeconds(3), true, status);
			}
		}
		
		// Remove the mindshield from the victim
		RemComp<MindShieldComponent>(victim);
		
		// Now proceed with the normal sacrifice
		_SacrificeIntoShell(victim, user, shell, cultistsInRange);
	}

	private void _SacrificeIntoShell(EntityUid victim, EntityUid user, EntityUid shell, EntityUid[] cultistsInRange)
	{
		// Get the victim's mind
		EntityUid? mindId = CompOrNull<MindContainerComponent>(victim)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail-nosoul"),
				user, user, PopupType.MediumCaution
			);
			return;
		}

		// Check if enough cultists are present
		// Require at least 1 cultist (matching the regular sacrifice requirement)
		if (cultistsInRange.Length < 1)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				user, user, PopupType.MediumCaution
			);
			return;
		}

		// Perform the sacrifice ritual announcement
		foreach (EntityUid invoker in cultistsInRange)
		{
			// Make cultists speak the ritual words
			// This mimics the behavior in BloodCultRuleSystem
			if (TryComp<BloodCultistComponent>(invoker, out var _))
			{
				// Speak ritual words
			}
		}

		// Get shell coordinates
		var shellCoordinates = Transform(shell).Coordinates;
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), shellCoordinates);
		
		// Delete the shell and spawn the juggernaut
		QueueDel(shell);
		var juggernaut = Spawn("MobBloodCultJuggernaut", shellCoordinates);
		
		// Get the juggernaut's body container
		if (_container.TryGetContainer(juggernaut, "juggernaut_body_container", out var container))
		{
			// Insert the victim's body into the juggernaut
			_container.Insert(victim, container);
		}
		
		// Transfer mind from victim to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, mind:mindComp);
		
		// Play transformation audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), shellCoordinates);
		
		// Notify the cultists
		_popupSystem.PopupEntity(
			Loc.GetString("cult-juggernaut-created"),
			user, user, PopupType.Large
		);
	}
	}
}
