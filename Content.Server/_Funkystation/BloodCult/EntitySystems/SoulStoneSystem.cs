// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction.Events;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mobs.Systems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class SoulStoneSystem : EntitySystem
{
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly BloodCultRuleSystem _cultRuleSystem = default!;
	[Dependency] private readonly BloodCultConstructSystem _constructSystem = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly IEntityManager _entityManager = default!;

	private EntityQuery<ShadeComponent> _shadeQuery;

	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<SoulStoneComponent, AfterInteractEvent>(OnTryCaptureSoul);
		SubscribeLocalEvent<SoulStoneComponent, UseInHandEvent>(OnUseInHand);

		_shadeQuery = GetEntityQuery<ShadeComponent>();
	}

	private void OnTryCaptureSoul(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
	{
		if (args.Handled
			|| !args.CanReach
			|| !args.ClickLocation.IsValid(_entityManager)
			|| !TryComp<BloodCultistComponent>(args.User, out var cultist) // ensure user is cultist
			|| HasComp<ActiveDoAfterComponent>(args.User)
			|| args.Target == null)
			return;

		if (!_shadeQuery.HasComponent(args.Target))
		{
			_constructSystem.TryApplySoulStone(ent, ref args);
			return;
		}

		args.Handled = true;

		if (args.Target != null && !_mobState.IsDead((EntityUid)args.Target) && TryComp<MindContainerComponent>(args.Target, out var mindContainer))
		{
			if (mindContainer.Mind != null && TryComp<MindComponent>((EntityUid)mindContainer.Mind, out var mindComp))
			{
				var coordinates = Transform((EntityUid)args.Target).Coordinates;
				_mind.TransferTo((EntityUid)mindContainer.Mind, ent, mind:mindComp);
				_audioSystem.PlayPvs("/Audio/Magic/blink.ogg", coordinates);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-shade-recalled"),
					args.User, args.User, PopupType.SmallCaution
				);
				QueueDel(args.Target);
			}
		}
	}

	private void OnUseInHand(Entity<SoulStoneComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

		args.Handled = true;

		if (TryComp<BloodCultistComponent>(args.User, out var _) && TryComp<MindContainerComponent>(ent, out var mindContainer))
		{
			if (mindContainer.Mind != null && TryComp<MindComponent>((EntityUid)mindContainer.Mind, out var mindComp))
			{
				var coordinates = Transform((EntityUid)args.User).Coordinates;
				var construct = Spawn("MobBloodCultShade", coordinates);
				_mind.TransferTo((EntityUid)mindContainer.Mind, construct, mind:mindComp);
				_audioSystem.PlayPvs("/Audio/Magic/blink.ogg", coordinates);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-shade-summoned"),
					args.User, args.User, PopupType.SmallCaution
				);
				string summonerName = _entityManager.GetComponent<MetaDataComponent>(args.User).EntityName;
				_cultRuleSystem.AnnounceToCultist(Loc.GetString("cult-shade-servant", ("name", summonerName)), construct);
			}
			else
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-soulstone-empty"),
					args.User, args.User, PopupType.SmallCaution
				);
			}
		}
	}
}
