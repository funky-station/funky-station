// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
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

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class ReviveCultistOnTriggerSystem : EntitySystem
	{
		[Dependency] private readonly EntityManager _entManager = default!;
		[Dependency] private readonly SharedTransformSystem _transform = default!;
		[Dependency] private readonly MapSystem _mapSystem = default!;
		[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
		[Dependency] private readonly DamageableSystem _damageableSystem = default!;
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly IPrototypeManager _protoMan = default!;
		[Dependency] private readonly IMapManager _mapManager = default!;

		[Dependency] private readonly EntityLookupSystem _lookup = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
		[Dependency] private readonly BloodCultistSystem _bloodCultist = default!;

		public override void Initialize()
		{
			base.Initialize();
			SubscribeLocalEvent<ReviveCultistOnTriggerComponent, TriggerEvent>(HandleReviveTrigger);
		}

		private void HandleReviveTrigger(EntityUid uid, ReviveCultistOnTriggerComponent component, TriggerEvent args)
		{
			if (args.User != null && !HasComp<BloodCultistComponent>(args.User))
				return;
			var lookup = _lookup.GetEntitiesInRange(uid, component.ReviveRange);
			foreach (var look in lookup)
			{
				if (HasComp<BloodCultistComponent>(look))
				{
					var dead = _mobState.IsDead(look);
					var hasUserId = CompOrNull<MindComponent>(CompOrNull<MindContainerComponent>(look)?.Mind)?.UserId;
					if (dead)
					{
						_bloodCultist.UseReviveRune(look, args.User, uid);
						break;
					}
					else if (hasUserId == null)
					{
						_bloodCultist.UseGhostifyRune(look, args.User, uid);
						break;
					}
				}
			}
			args.Handled = true;
		}
	}
}
