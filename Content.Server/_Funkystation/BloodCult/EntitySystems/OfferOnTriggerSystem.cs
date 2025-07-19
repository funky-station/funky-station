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
using Content.Shared.Body.Systems;
using Content.Shared.Roles;
using Content.Server.Roles;

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class OfferOnTriggerSystem : EntitySystem
	{
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly EntityLookupSystem _lookup = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
		[Dependency] private readonly SharedBodySystem _body = default!;
		[Dependency] private readonly SharedRoleSystem _role = default!;
		[Dependency] private readonly BloodCultistSystem _bloodCultist = default!;

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
			foreach (var look in offerLookup)
			{
				if (HasComp<HumanoidAppearanceComponent>(look))
					humanoids.Add(look);
				else if (HasComp<BrainComponent>(look))
					brains.Add(look);
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
					_bloodCultist.UseConvertRune(offerable, user, uid, cultistsInRange);
				}
				else if (_CanBeSacrificed(offerable))
				{
					_bloodCultist.UseSacrificeRune(offerable, user, uid, cultistsInRange);
				}
				else
				{
					_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-fail-mindshielded"),
							user, user, PopupType.MediumCaution
						);
				}
			}
			args.Handled = true;
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
			return !_mobState.IsDead(uid) &&  // must not be dead
				!HasComp<MindShieldComponent>(uid);  // must not be mindshielded
		}

		private bool _CanBeSacrificed(EntityUid uid)
		{
			return true;  // Always.
		}
	}
}
