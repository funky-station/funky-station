// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 mkanke-real <mikekanke@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

//using Content.Shared.Tag;
using System.Linq;
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
	[Dependency] private readonly BloodCultRuleSystem _cultRule = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly IEntityManager _entManager = default!;

	private EntityQuery<BloodCultRuneComponent> _runeQuery;

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

		// Third, verify that a new rune can be placed here.
		if (args.User != target
			|| !args.ClickLocation.IsValid(EntityManager)
			|| !CanPlaceRuneAt(args.ClickLocation, out var location))
			return;

		var timeToCarve = ent.Comp.TimeToCarve;

		// Third and a half, if this is a TearVeilRune, do a special location check.
		if (ent.Comp.Rune == "TearVeilRune")
		{
			// If they have not yet confirmed the summon location, check to make sure
			// that this is a valid one and ask them to confirm.
			if (cultist.LocationForSummon == null)
			{
				cultist.TryingDrawTearVeil = true;
				return;
			}

			// If they made it to here, they are trying to draw a confirmed tear veil rune.
			// Ensure that the location is valid.
			WeakVeilLocation locationForSummon = (WeakVeilLocation)(cultist.LocationForSummon);
			if (!locationForSummon.Coordinates.InRange(_entManager, Transform(args.User).Coordinates, locationForSummon.ValidRadius))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-drawing-wronglocation", ("name", locationForSummon.Name)),
					args.User, args.User, PopupType.MediumCaution
				);
				return;
			}

			// Check to make sure no other tear veil runes already exist.
			var summonRunes = AllEntityQuery<TearVeilComponent, BloodCultRuneComponent>();
			while (summonRunes.MoveNext(out var uid, out _, out var _))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-drawing-alreadyexists"),
					args.User, args.User, PopupType.MediumCaution
				);
				return;
			}

			timeToCarve = 45.0f;
		}

		// Fourth, raise an event to place a rune here.
		var rune = Spawn(ent.Comp.InProgress, location);
		var dargs = new DoAfterArgs(EntityManager, args.User, timeToCarve, new DrawRuneDoAfterEvent(
			ent, rune, location, ent.Comp.Rune, ent.Comp.BleedOnCarve, ent.Comp.CarveSound), args.User
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

	private void OnRuneDoAfter(Entity<DamageableComponent> ent, ref DrawRuneDoAfterEvent ev)
    {
		// Delete the animation
        QueueDel(ev.Rune);

		DamageSpecifier appliedDamageSpecifier;
		if (ent.Comp.Damage.DamageDict.ContainsKey("Bloodloss"))
			appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Bloodloss"), FixedPoint2.New(ev.BleedOnCarve));
		else if (ent.Comp.Damage.DamageDict.ContainsKey("Ion"))
			appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Ion"), FixedPoint2.New(ev.BleedOnCarve));
		else
			appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(ev.BleedOnCarve));

        if (!ev.Cancelled)
		{
			var gridUid = _transform.GetGrid(ev.Coords);
			if (!TryComp<MapGridComponent>(gridUid, out var grid))
			{
				return;
			}
			var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, ev.Coords);

		var rune = Spawn(ev.EntityId, ev.Coords);  // Spawn the final rune

		if (gridUid != null)
		{
			var runeTransform = Transform(rune);
			_transform.AnchorEntity((rune, runeTransform), ((EntityUid)gridUid, grid), targetTile.GridIndices);
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
			_audioSystem.PlayPvs(ev.CarveSound, ent);
		}
		else
		{
			QueueDel(rune);
		}
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
			_audioSystem.PlayPvs("/Audio/Effects/lightburn.ogg", Transform(args.User).Coordinates);
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
}
