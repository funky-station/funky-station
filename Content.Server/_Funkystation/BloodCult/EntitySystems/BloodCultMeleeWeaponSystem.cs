using System.Linq;
using Robust.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Server.Hands.Systems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Server.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodCultMeleeWeaponSystem : EntitySystem
{
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly HandsSystem _hands = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;

	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<BloodCultMeleeWeaponComponent, MeleeHitEvent>(OnBloodCultMeleeHit);
	}

	private void OnBloodCultMeleeHit(EntityUid uid, BloodCultMeleeWeaponComponent comp, MeleeHitEvent args)
	{
		bool blockedByChaplain = false;
		bool blockedByCultist = false;

		if (args.IsHit &&
			args.HitEntities.Any())
		{
			if (args.HitEntities.Any(r => HasComp<CultResistantComponent>(r)))
				blockedByChaplain = true;
			if (args.HitEntities.Any(r => HasComp<BloodCultistComponent>(r)) || args.HitEntities.Any(r => HasComp<BloodCultConstructComponent>(r)))
				blockedByCultist = true;
		}

		if (blockedByChaplain || blockedByCultist)
			args.Handled = true;

		if (blockedByChaplain)
		{
			_popupSystem.PopupEntity(
					Loc.GetString("cult-attack-repelled"),
					args.User, args.User, PopupType.MediumCaution
				);
			var coordinates = Transform(args.User).Coordinates;
			_audioSystem.PlayPvs("/Audio/Effects/holy.ogg", coordinates);
			var offsetRandomCoordinates = coordinates.Offset(_random.NextVector2(1f, 1.5f));
            _hands.ThrowHeldItem(args.User, offsetRandomCoordinates);
		}
		if (blockedByCultist)
		{
			_popupSystem.PopupEntity(
					Loc.GetString("cult-attack-teamhit"),
					args.User, args.User, PopupType.MediumCaution
				);
		}
	}
}
