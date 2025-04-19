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
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Server.GameTicking.Rules;

namespace Content.Server.BloodCult.EntitySystems
{
	public sealed partial class BarrierOnTriggerSystem : EntitySystem
	{
		[Dependency] private readonly EntityManager _entManager = default!;
		[Dependency] private readonly SharedTransformSystem _transform = default!;
		[Dependency] private readonly MapSystem _mapSystem = default!;
		[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
		[Dependency] private readonly DamageableSystem _damageableSystem = default!;
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly IPrototypeManager _protoMan = default!;
		[Dependency] private readonly IMapManager _mapManager = default!;
		[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;

		private EntityQuery<BloodCultRuneComponent> _runeQuery;
		private EntityQuery<ForceBarrierComponent> _barrierQuery;

		public override void Initialize()
		{
			base.Initialize();

			_runeQuery = GetEntityQuery<BloodCultRuneComponent>();
			_barrierQuery = GetEntityQuery<ForceBarrierComponent>();

			SubscribeLocalEvent<BarrierOnTriggerComponent, TriggerEvent>(HandleBarrierTrigger);
		}

		private void HandleBarrierTrigger(EntityUid uid, BarrierOnTriggerComponent component, TriggerEvent args)
		{
			if (_entManager.TryGetComponent<TransformComponent>(uid, out var xform))
			{
				if (args.User != null && !HasComp<BloodCultistComponent>(args.User))
					return;
				if (CanPlaceBarrierAt(xform.Coordinates, out var location))
				{
					var gridUid = _transform.GetGrid(location);
					if (!TryComp<MapGridComponent>(gridUid, out var grid))
					{
						return;
					}
					var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);

					List<EntityCoordinates> dL = new List<EntityCoordinates>();
					Queue<EntityCoordinates> sQ = new Queue<EntityCoordinates>();
					sQ.Enqueue(location);

					int damageOnActivate = 0;
					while (sQ.Count > 0)
					{
						damageOnActivate = damageOnActivate + component.DamageOnActivate;//1;
						var currentLocation = sQ.Dequeue();
						dL.Add(currentLocation);
						SpawnAtLocation(currentLocation);

						if(CanPlaceBarrierAt(currentLocation.Offset(new Vector2(-1.2f, 0f)), out var nextLoc1))
						{
							if (!dL.Contains(nextLoc1))
								sQ.Enqueue(nextLoc1);
						}
						if (CanPlaceBarrierAt(currentLocation.Offset(new Vector2(1.2f, 0f)), out var nextLoc2))
						{
							if (!dL.Contains(nextLoc2))
								sQ.Enqueue(nextLoc2);
						}
						if (CanPlaceBarrierAt(currentLocation.Offset(new Vector2(0f, -1.2f)), out var nextLoc3))
						{
							if (!dL.Contains(nextLoc3))
								sQ.Enqueue(nextLoc3);
						}
						if (CanPlaceBarrierAt(currentLocation.Offset(new Vector2(0f, 1.2f)), out var nextLoc4))
						{
							if (!dL.Contains(nextLoc4))
								sQ.Enqueue(nextLoc4);
						}
					}

					if (args.User != null)
					{
						var user = (EntityUid) args.User;
						_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-blood-drain"),
							user, user, PopupType.MediumCaution
						);
						_bloodCultRule.Speak(user, Loc.GetString("cult-invocation-barrier"));

						TryComp<DamageableComponent>(user, out var damComp);

						DamageSpecifier appliedDamageSpecifier;
						appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(damageOnActivate));

						_damageableSystem.TryChangeDamage(user, appliedDamageSpecifier, true, origin: user);
					}
				}
			}
			args.Handled = true;
		}

		private void SpawnAtLocation(EntityCoordinates inLocation)
		{
			var gridUid = _transform.GetGrid(inLocation);
			if (!TryComp<MapGridComponent>(gridUid, out var grid))
			{
				return;
			}
			var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, inLocation);

			var barrier = Spawn("ForceBarrier", inLocation);

			if (gridUid != null && TryComp<TransformComponent>(barrier, out var barrierTransform))
			{
				_transform.AnchorEntity((barrier, barrierTransform), ((EntityUid)gridUid, grid), targetTile.GridIndices);
				_audioSystem.PlayPvs("/Audio/Effects/inneranomaly.ogg", inLocation);
			}
			else
			{
				QueueDel(barrier);
			}
		}

		private bool CanPlaceBarrierAt(EntityCoordinates clickedAt, out EntityCoordinates location)
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

			bool didFindBarrier = false;
			bool didFindRune = false;
			foreach (var possibleEnt in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices))
			{
				if (_barrierQuery.HasComponent(possibleEnt))
					didFindBarrier = true;
				if (_runeQuery.HasComponent(possibleEnt) && TryComp<BarrierOnTriggerComponent>(possibleEnt, out var _))
					didFindRune = true;
			}
			return! didFindBarrier && didFindRune;
		}
	}
}
