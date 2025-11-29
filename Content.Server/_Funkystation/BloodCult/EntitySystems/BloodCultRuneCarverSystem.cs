// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 mkanke-real <mikekanke@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

//using Content.Shared.Tag;
using System;
using System.Linq;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
//using Content.Shared.Transform;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class BloodCultRuneCarverSystem : EntitySystem
{
	[Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
	[Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly BloodstreamSystem _bloodstream = default!;

	private EntityQuery<BloodCultRuneComponent> _runeQuery;
	private uint _nextEffectId = 1;

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<BloodCultRuneCarverComponent, MapInitEvent>(OnMapInit);

		SubscribeLocalEvent<BloodCultRuneCarverComponent, AfterInteractEvent>(OnTryDrawRune);
		SubscribeLocalEvent<DamageableComponent, DrawRuneDoAfterEvent>(OnRuneDoAfter);
		SubscribeLocalEvent<BloodCultRuneCarverComponent, UseInHandEvent>(OnUseInHand);
		//SubscribeLocalEvent<HereticRitualRuneComponent, InteractHandEvent>(OnInteract);

		SubscribeLocalEvent<BloodCultRuneCarverComponent, GetVerbsEvent<InteractionVerb>>(OnVerb);

		SubscribeLocalEvent<BloodCultRuneCarverComponent, RunesMessage>(OnRuneChosenMessage);

		SubscribeLocalEvent<BloodCultRuneCarverComponent, GotEquippedHandEvent>(OnEquipped);

		_runeQuery = GetEntityQuery<BloodCultRuneComponent>();
	}

	private void OnMapInit(EntityUid uid, BloodCultRuneCarverComponent component, MapInitEvent args)
	{

	}

	#region UserInterface
	private void OnVerb(EntityUid uid, BloodCultRuneCarverComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || component.User != args.User)
            return;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = "Example Text",//Loc.GetString("chameleon-component-verb-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => TryOpenUi(uid, args.User, component)
        });
    }

	private void OnRuneChosenMessage(Entity<BloodCultRuneCarverComponent> ent, ref RunesMessage args)
	{
		//var user = args.Actor;
		if (!BloodCultRuneCarverComponent.ValidRunes.Contains(args.ProtoId))
			return;
		ent.Comp.Rune = args.ProtoId;
        ent.Comp.InProgress = args.ProtoId + "_drawing";
    }

	private void TryOpenUi(EntityUid uid, EntityUid user, BloodCultRuneCarverComponent? component = null)
	{
		if (!HasComp<BloodCultistComponent>(user) || !Resolve(uid, ref component) || !TryComp(user, out ActorComponent? actor))
			return;
		_uiSystem.TryToggleUi(uid, RunesUiKey.Key, actor.PlayerSession);
	}

	private void UpdateUi(EntityUid uid, BloodCultRuneCarverComponent? component = null)
	{
		if (!Resolve(uid, ref component))
			return;

		var state = new RuneUserInterfaceState(component.Rune);
		_uiSystem.SetUiState(uid, RunesUiKey.Key, state);
	}
	#endregion

	private void OnTryDrawRune(Entity<BloodCultRuneCarverComponent> ent, ref AfterInteractEvent args)
    {
		// First, make sure this is a valid use at all.
		if (args.Handled
			|| !args.CanReach
			|| !args.ClickLocation.IsValid(EntityManager)
			|| !TryComp<BloodCultistComponent>(args.User, out var cultist) // ensure user is cultist
			|| HasComp<ActiveDoAfterComponent>(args.User)
			|| args.Target == null)
			return;
		args.Handled = true;

		var target = (EntityUid) args.Target;

		// Second, check to see if we are using the carver to delete a rune.
		if (_runeQuery.HasComponent(target))
        {
            QueueDel(target);
            return;
        }

		// Third, if clicking on yourself and no rune is selected, open the selection UI
		if (args.User == target && string.IsNullOrEmpty(ent.Comp.Rune))
		{
			TryOpenUi(ent, args.User, ent.Comp);
			return;
		}

		// Fourth, verify that a new rune can be placed here.
		if (args.User != target
			|| !args.ClickLocation.IsValid(EntityManager)
			|| !CanPlaceRuneAt(args.ClickLocation, out var location))
			return;

		var timeToCarve = ent.Comp.TimeToCarve;

		// Third and a half, if this is a TearVeilRune, do a special location check.
		if (ent.Comp.Rune == "TearVeilRune")
		{
			if (!TryGetValidVeilLocation(args.ClickLocation, out var locationForSummon))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-drawing-toostrong"),
					args.User, args.User, PopupType.MediumCaution
				);
				return;
			}

			cultist.LocationForSummon = locationForSummon;

			// Allow multiple tear veil runes to exist (one per location)
			// Check if a rune already exists at THIS specific location
			var summonRunes = AllEntityQuery<TearVeilComponent, BloodCultRuneComponent, TransformComponent>();
			while (summonRunes.MoveNext(out var existingRuneUid, out _, out var _, out var runeXform))
			{
				// Check if this existing rune is in range of the current location we're trying to draw at
				if (_transform.InRange(runeXform.Coordinates, locationForSummon.Coordinates, locationForSummon.ValidRadius))
				{
					_popupSystem.PopupEntity(
						Loc.GetString("cult-veil-drawing-alreadyexists-location", ("name", locationForSummon.Name)),
						args.User, args.User, PopupType.MediumCaution
					);
					return;
				}
			}

			timeToCarve = 45.0f;
		}

		// Fourth, raise an effect event to place a rune here.
		uint? effectId = null;
		var duration = TimeSpan.FromSeconds(timeToCarve);
		if (TryGetRuneEffectPrototype(ent.Comp.Rune, out var effectPrototype))
		{
			effectId = _nextEffectId++;
			RaiseEffectEvent(effectId.Value, effectPrototype, location, RuneEffectAction.Start, duration);
		}

		var dargs = new DoAfterArgs(EntityManager, args.User, timeToCarve, new DrawRuneDoAfterEvent(
			ent, EntityUid.Invalid, location, ent.Comp.Rune, ent.Comp.BleedOnCarve, ent.Comp.CarveSound, effectId, duration), args.User
		)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
			BreakOnDropItem = true,
            CancelDuplicate = false,
        };

		if (_protoMan.TryIndex(ent.Comp.Rune, out var ritualPrototype))
			_popupSystem.PopupEntity(
				Loc.GetString("cult-rune-drawing-vowel-first") +
				("aeiou".Contains(ritualPrototype.Name.ToLower()[0]) ? "n" : "") +
				" " + ritualPrototype.Name + " " + Loc.GetString("cult-rune-drawing-vowel-second"),
				args.User, args.User, PopupType.MediumCaution
			);
		else
			_popupSystem.PopupEntity(
				Loc.GetString("cult-rune-drawing-novowel"),
				args.User, args.User, PopupType.MediumCaution
			);
		_doAfter.TryStartDoAfter(dargs);
    }

	private bool TryGetValidVeilLocation(EntityCoordinates placement, out WeakVeilLocation location)
	{
		var ruleQuery = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		while (ruleQuery.MoveNext(out _, out var ruleComp, out _))
		{
			if (ruleComp.WeakVeil1 != null)
			{
				var candidate = (WeakVeilLocation)ruleComp.WeakVeil1;
				if (_transform.InRange(candidate.Coordinates, placement, candidate.ValidRadius))
				{
					location = candidate;
					return true;
				}
			}
			if (ruleComp.WeakVeil2 != null)
			{
				var candidate = (WeakVeilLocation)ruleComp.WeakVeil2;
				if (_transform.InRange(candidate.Coordinates, placement, candidate.ValidRadius))
				{
					location = candidate;
					return true;
				}
			}
			if (ruleComp.WeakVeil3 != null)
			{
				var candidate = (WeakVeilLocation)ruleComp.WeakVeil3;
				if (_transform.InRange(candidate.Coordinates, placement, candidate.ValidRadius))
				{
					location = candidate;
					return true;
				}
			}
		}

		location = default;
		return false;
	}

	private void OnRuneDoAfter(Entity<DamageableComponent> ent, ref DrawRuneDoAfterEvent ev)
    {
		// Delete the animation
        QueueDel(ev.Rune);

		// Apply bloodloss damage + bleeding + slashing damage when drawing runes
		DamageSpecifier appliedDamageSpecifier;
		if (ent.Comp.Damage.DamageDict.ContainsKey("Bloodloss"))
		{
			// Organic entities: bloodloss + slash damage
			appliedDamageSpecifier = new DamageSpecifier();
			appliedDamageSpecifier.DamageDict.Add("Bloodloss", FixedPoint2.New(ev.BleedOnCarve));
			appliedDamageSpecifier.DamageDict.Add("Slash", FixedPoint2.New(10));
			
			// Add bleeding effect
			if (TryComp<BloodstreamComponent>(ent, out var bloodstream))
			{
				_bloodstream.TryModifyBleedAmount(ent, ev.BleedOnCarve / 10f, bloodstream);
			}
		}
		else if (ent.Comp.Damage.DamageDict.ContainsKey("Ion"))
			appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Ion"), FixedPoint2.New(ev.BleedOnCarve));
		else
			appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(ev.BleedOnCarve));

        if (ev.EffectId.HasValue)
            RaiseEffectEvent(ev.EffectId.Value, null, ev.Coords, RuneEffectAction.Stop, TimeSpan.Zero);

        if (!ev.Cancelled)
		{
			var gridUid = _transform.GetGrid(ev.Coords);
			if (!TryComp<MapGridComponent>(gridUid, out var grid))
			{
				return;
			}
			var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, ev.Coords);

			var rune = Spawn(ev.EntityId, ev.Coords);  // Spawn the final rune

			if (gridUid != null && TryComp<TransformComponent>(rune, out var runeTransform))
			{
				_transform.AnchorEntity((rune, runeTransform), ((EntityUid)gridUid, grid), targetTile.GridIndices);
				_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
				_audioSystem.PlayPvs(ev.CarveSound, ent);
				
				// Clear the selected rune so the UI opens automatically on next click
				if (TryComp<BloodCultRuneCarverComponent>(ev.CarverUid, out var carverComp))
				{
					carverComp.Rune = "";
					carverComp.InProgress = "";
				}
			}
			else
			{
				QueueDel(rune);
			}
		}
        else
        {
            return;
        }
    }

	private void OnUseInHand(Entity<BloodCultRuneCarverComponent> ent, ref UseInHandEvent ev)
	{
	}

	private void OnEquipped(EntityUid uid, BloodCultRuneCarverComponent component, GotEquippedHandEvent args)
	{
		if (!HasComp<BloodCultistComponent>(args.User))
		{
			QueueDel(uid);
		Spawn("Ash", Transform(args.User).Coordinates);
		_popupSystem.PopupEntity(
			Loc.GetString("cult-dagger-equip-fail"),
			args.User, args.User, PopupType.SmallCaution
		);
		_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Effects/lightburn.ogg"), Transform(args.User).Coordinates);
		}
	}

	private bool CanPlaceRuneAt(EntityCoordinates clickedAt, out EntityCoordinates location)
	{
		location = clickedAt.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapManager);
		var gridUid = _transform.GetGrid(location);
		if (!TryComp<MapGridComponent>(gridUid, out var grid))
        {
            return false;
        }
		var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);

		// This does not work, but should.
		//if (_mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices).Any(_runeQuery.HasComponent))
        //{
        //    return;
        //}

		foreach (var possibleEnt in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices))
		{
			if (_runeQuery.HasComponent(possibleEnt))
				return false;
		}
		return true;
	}

    private bool TryGetRuneEffectPrototype(string runePrototype, out string? effectPrototype)
    {
        switch (runePrototype)
        {
            case "BarrierRune":
                effectPrototype = "FxBloodCultRuneBarrier";
                return true;
            case "EmpoweringRune":
                effectPrototype = "FxBloodCultRuneEmpowering";
                return true;
            case "OfferingRune":
                effectPrototype = "FxBloodCultRuneOffering";
                return true;
            case "ReviveRune":
                effectPrototype = "FxBloodCultRuneRevive";
                return true;
		case "TearVeilRune":
				effectPrototype = "TearVeilRune_drawing";
                return true;
		case "SummoningRune":
				effectPrototype = "SummoningRune_drawing";
                return true;
            default:
                effectPrototype = null;
                return false;
        }
    }

    private void RaiseEffectEvent(uint effectId, string? prototype, EntityCoordinates coords, RuneEffectAction action, TimeSpan duration)
    {
        var netCoords = GetNetCoordinates(coords);
        var ev = new RuneDrawingEffectEvent(effectId, prototype, netCoords, action, duration);
        RaiseNetworkEvent(ev, Filter.Pvs(coords, entityMan: EntityManager));
    }

    private NetCoordinates GetNetCoordinates(EntityCoordinates coords)
    {
        var netEntity = EntityManager.GetNetEntity(coords.EntityId);
        return new NetCoordinates(netEntity, coords.Position);
    }
}
