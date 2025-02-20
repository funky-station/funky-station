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

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class OfferOnTriggerSystem : EntitySystem
	{
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly EntityLookupSystem _lookup = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
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

			var offerLookup = _lookup.GetEntitiesInRange(uid, component.OfferRange);
			var invokeLookup = _lookup.GetEntitiesInRange(uid, component.InvokeRange);
			EntityUid[] cultistsInRange = Array.FindAll(invokeLookup.ToArray(), item => (HasComp<BloodCultistComponent>(item) && !_mobState.IsDead(item)));
			Console.WriteLine("TOTAL CULSTISTS IN RANGE:");
			Console.WriteLine(cultistsInRange.Length);
			foreach (var look in offerLookup)
			{
				if (!HasComp<HumanoidAppearanceComponent>(look))
					continue;

				if (!_IsValidTarget(look))
				{
					_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-fail-nosoul"),
							user, user, PopupType.MediumCaution
						);
					break;
				}
				else if (HasComp<BloodCultistComponent>(look))
				{
					_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-fail-teamkill"),
							user, user, PopupType.MediumCaution
						);
					break;
				}
				else if (_CanBeConverted(look))
				{
					_bloodCultist.UseConvertRune(look, user, uid, cultistsInRange);
					break;
				}
				else if (_CanBeSacrificed(look))
				{
					_bloodCultist.UseSacrificeRune(look, user, uid, cultistsInRange);
					break;
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

		private bool _IsValidTarget(EntityUid uid)
		{
			return TryComp(uid, out MindContainerComponent? mindContainer) &&
				mindContainer?.Mind != null;  // must have a soul
		}

		private bool _CanBeConverted(EntityUid uid)
		{
			return !_mobState.IsDead(uid) &&  // must not be dead
				!HasComp<MindShieldComponent>(uid);  // must not be mindshielded
		}

		private bool _CanBeSacrificed(EntityUid uid)
		{
			return (_mobState.IsDead(uid));  // must be dead
		}
	}
}
