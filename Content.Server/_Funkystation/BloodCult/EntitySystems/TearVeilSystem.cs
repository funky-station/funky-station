// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
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
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.BloodCult;

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class TearVeilSystem : EntitySystem
	{
		[Dependency] private readonly EntityManager _entManager = default!;
		[Dependency] private readonly SharedTransformSystem _transform = default!;
		[Dependency] private readonly MapSystem _mapSystem = default!;
		[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
		[Dependency] private readonly DamageableSystem _damageableSystem = default!;
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly IPrototypeManager _protoMan = default!;
		[Dependency] private readonly IMapManager _mapManager = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
		[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
		[Dependency] private readonly EntityLookupSystem _lookup = default!;

		private EntityQuery<BloodCultRuneComponent> _runeQuery;
		private EntityQuery<ForceBarrierComponent> _barrierQuery;

		public override void Initialize()
		{
			base.Initialize();

			_runeQuery = GetEntityQuery<BloodCultRuneComponent>();
			_barrierQuery = GetEntityQuery<ForceBarrierComponent>();

			SubscribeLocalEvent<TearVeilComponent, TriggerEvent>(HandleTearVeil);
		}

		private void HandleTearVeil(EntityUid uid, TearVeilComponent component, TriggerEvent args)
		{
			if (_entManager.TryGetComponent<TransformComponent>(uid, out var xform) && TryComp<BloodCultistComponent>(args.User, out var bloodCultist))
			{
				if (CanTearVeil(xform.Coordinates, out List<EntityUid> _))
				{
					bloodCultist.NarsieSummoned = xform.Coordinates;
				}
				else
				{
					bloodCultist.FailedNarsieSummon = true;
				}
			}
			args.Handled = true;
		}

		private bool CanTearVeil(EntityCoordinates triggeredAt, out List<EntityUid> cultists)
		{
			cultists = new List<EntityUid>();

			var bottomLeft = triggeredAt.Offset(new Vector2(-1,-1));
			var left = triggeredAt.Offset(new Vector2(-1,0));
			var topLeft = triggeredAt.Offset(new Vector2(-1,1));

			var bottom = triggeredAt.Offset(new Vector2(0, -1));
			var top = triggeredAt.Offset(new Vector2(0, 1));

			var bottomRight = triggeredAt.Offset(new Vector2(1,-1));
			var right = triggeredAt.Offset(new Vector2(1,0));
			var topRight = triggeredAt.Offset(new Vector2(1,1));

			var lookup0 = _lookup.GetEntitiesInRange(triggeredAt, 1.1f);
			var lookup1 = _lookup.GetEntitiesInRange(topLeft, 0.225f);
			var lookup2 = _lookup.GetEntitiesInRange(top, 0.225f);
			var lookup3 = _lookup.GetEntitiesInRange(topRight, 0.225f);
			var lookup4 = _lookup.GetEntitiesInRange(left, 0.225f);
			var lookup5 = _lookup.GetEntitiesInRange(triggeredAt, 0.225f);
			var lookup6 = _lookup.GetEntitiesInRange(right, 0.225f);
			var lookup7 = _lookup.GetEntitiesInRange(bottomLeft, 0.225f);
			var lookup8 = _lookup.GetEntitiesInRange(bottom, 0.225f);
			var lookup9 = _lookup.GetEntitiesInRange(bottomRight, 0.225f);
			//bool didFindCultist = false;//never used

			int cultistsFound = 0;
			bool found1 = false;
			bool found2 = false;
			bool found3 = false;
			bool found4 = false;
			bool found5 = false;
			bool found6 = false;
			bool found7 = false;
			bool found8 = false;
			bool found9 = false;

			foreach (var look in lookup0)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					cultistsFound = cultistsFound + 1;
					cultists.Add(look);
				}
			}
			foreach (var look in lookup1)
			{
				// TODO: Support summoned ghosts to help with this summoning
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found1 = true;
					break;
				}
			}
			foreach (var look in lookup2)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found2 = true;
					break;
				}
			}
			foreach (var look in lookup3)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found3 = true;
					break;
				}
			}
			foreach (var look in lookup4)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found4 = true;
					break;
				}
			}
			foreach (var look in lookup5)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found5 = true;
					break;
				}
			}
			foreach (var look in lookup6)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found6 = true;
					break;
				}
			}
			foreach (var look in lookup7)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found7 = true;
					break;
				}
			}
			foreach (var look in lookup8)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found8 = true;
					break;
				}
			}
			foreach (var look in lookup9)
			{
				if ((TryComp<BloodCultistComponent>(look, out var _) || TryComp<BloodCultConstructComponent>(look, out var _)) && !_mobState.IsDead(look))
				{
					found9 = true;
					break;
				}
			}

			bool[] tilesArray = new bool[9] { found1, found2, found3, found4, found5, found6, found7, found8, found9 };
			tilesArray.Where(c => c).Count();

			return (cultistsFound>=9) && (tilesArray.Where(c => c).Count() >= 6);
		}
	}
}
