//using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.DoAfter;
//using Content.Shared.Transform;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class BloodCultRuneCarverSystem : EntitySystem
{
	[Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;

	private EntityQuery<BloodCultRuneComponent> _runeQuery;

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<BloodCultRuneCarverComponent, AfterInteractEvent>(OnTryDrawRune);
		SubscribeLocalEvent<DamageableComponent, DrawRuneDoAfterEvent>(OnRuneDoAfter);
		SubscribeLocalEvent<BloodCultRuneCarverComponent, UseInHandEvent>(OnUseInHand);

		_runeQuery = GetEntityQuery<BloodCultRuneComponent>();
	}

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
		var dargs = new DoAfterArgs(EntityManager, args.User, ent.Comp.TimeToCarve, new DrawRuneDoAfterEvent(ent, rune, location, ent.Comp.Rune, ent.Comp.BleedOnCarve, ent.Comp.CarveSound), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
			BreakOnDropItem = true,
            CancelDuplicate = false,
        };
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
