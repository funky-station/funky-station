//using Content.Shared.Tag;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.DoAfter;
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
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly IPrototypeManager _prototypeManager = default!;

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
	}

	private void TryOpenUi(EntityUid uid, EntityUid user, BloodCultRuneCarverComponent? component = null)
	{
		if (!Resolve(uid, ref component))
			return;
		if (!TryComp(user, out ActorComponent? actor))
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
			//|| !TryComp<BloodCultistComponent>(args.User, out var cultist) // ensure user is cultist
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

		// Fourth, raise an event to place a rune here.
		var rune = Spawn(ent.Comp.InProgress, location);
		var dargs = new DoAfterArgs(EntityManager, args.User, ent.Comp.TimeToCarve, new DrawRuneDoAfterEvent(
			ent, rune, location, ent.Comp.Rune, ent.Comp.BleedOnCarve, ent.Comp.CarveSound), args.User
		)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
			BreakOnDropItem = true,
            CancelDuplicate = false,
        };

		if (_prototypeManager.TryIndex(ent.Comp.Rune, out var ritualPrototype))
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
			_mapSystem.AddToSnapGridCell(gridUid.Value, grid, targetTile.GridIndices, rune);
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
			_audioSystem.PlayPvs(ev.CarveSound, ent);
		}
    }

	private void OnUseInHand(Entity<BloodCultRuneCarverComponent> ent, ref UseInHandEvent ev)
	{
		Console.WriteLine("Used it in hand!");
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
