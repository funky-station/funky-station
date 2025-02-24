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
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
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
				if (CanTearVeil(xform.Coordinates, out List<Entity<BloodCultistComponent>> cultists))
				{
					bloodCultist.NarsieSummoned = xform.Coordinates;
				}
				else
				{
					bloodCultist.FailedNarsieSummon = true;
				}
				/*
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
				*/
			}
			args.Handled = true;
		}
/*
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
*/
		private bool CanTearVeil(EntityCoordinates triggeredAt, out List<Entity<BloodCultistComponent>> cultists)
		{
			cultists = new List<Entity<BloodCultistComponent>>();

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
			bool didFindCultist = false;

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
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					cultistsFound = cultistsFound + 1;
					cultists.Add((look, th));
				}
			}
			foreach (var look in lookup1)
			{
				// TODO: Support cult constructs and summoned ghosts to help with this summoning
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found1 = true;
					break;
				}
			}
			foreach (var look in lookup2)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found2 = true;
					break;
				}
			}
			foreach (var look in lookup3)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found3 = true;
					break;
				}
			}
			foreach (var look in lookup4)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found4 = true;
					break;
				}
			}
			foreach (var look in lookup5)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found5 = true;
					break;
				}
			}
			foreach (var look in lookup6)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found6 = true;
					break;
				}
			}
			foreach (var look in lookup7)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found7 = true;
					break;
				}
			}
			foreach (var look in lookup8)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found8 = true;
					break;
				}
			}
			foreach (var look in lookup9)
			{
				if (TryComp<BloodCultistComponent>(look, out var th))
				{
					found9 = true;
					break;
				}
			}

			return (cultistsFound>=9)&&found1&&found2&&found3&&found4&&found5&&found6&&found7&&found8&&found9;
		}
/*
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
*/
	}
}
