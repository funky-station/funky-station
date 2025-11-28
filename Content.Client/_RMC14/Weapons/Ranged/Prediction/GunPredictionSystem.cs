// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 Toastermeister <215405651+Toastermeister@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Client.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged.Prediction;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Fluids.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Client._RMC14.Weapons.Ranged.Prediction;

public sealed class GunPredictionSystem : SharedGunPredictionSystem
{
    public const string ProjectileFixture = "projectile";
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly Robust.Client.Physics.PhysicsSystem _physics = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ProjectileSystem _projectile = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<IgnorePredictionHideComponent> _ignorePredictionHideQuery;
    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        base.Initialize();

        _ignorePredictionHideQuery = GetEntityQuery<IgnorePredictionHideComponent>();
        _spriteQuery = GetEntityQuery<SpriteComponent>();

        SubscribeLocalEvent<PhysicsUpdateBeforeSolveEvent>(OnBeforeSolve);
        SubscribeLocalEvent<PhysicsUpdateAfterSolveEvent>(OnAfterSolve);
        SubscribeLocalEvent<RequestShootEvent>(OnShootRequest);

        SubscribeLocalEvent<PredictedProjectileClientComponent, UpdateIsPredictedEvent>(OnClientProjectileUpdateIsPredicted);
        SubscribeLocalEvent<PredictedProjectileClientComponent, StartCollideEvent>(OnClientProjectileStartCollide);

        SubscribeLocalEvent<PredictedProjectileServerComponent, ComponentStartup>(OnServerProjectileStartup);

        UpdatesBefore.Add(typeof(TransformSystem));
    }

    private void OnBeforeSolve(ref PhysicsUpdateBeforeSolveEvent ev)
    {
        var query = EntityQueryEnumerator<PredictedProjectileClientComponent>();
        while (query.MoveNext(out var uid, out var predicted))
        {
            predicted.Coordinates = Transform(uid).Coordinates;
        }
    }

    private void OnAfterSolve(ref PhysicsUpdateAfterSolveEvent ev)
    {
        var query = EntityQueryEnumerator<PredictedProjectileClientComponent>();
        while (query.MoveNext(out var uid, out var predicted))
        {
            if (_timing.IsFirstTimePredicted)
                continue;

            if (predicted.Coordinates is { } coordinates)
                _transform.SetCoordinates(uid, coordinates);

            predicted.Coordinates = null;
        }
    }

    private void OnShootRequest(RequestShootEvent ev, EntitySessionEventArgs args)
    {
        if (_timing.IsFirstTimePredicted)
            return;

        _gun.ShootRequested(ev.Gun, ev.Coordinates, ev.Target, null, args.SenderSession);
    }

    private void OnClientProjectileUpdateIsPredicted(Entity<PredictedProjectileClientComponent> ent, ref UpdateIsPredictedEvent args)
    {
        args.IsPredicted = true;
    }

    private void OnClientProjectileStartCollide(Entity<PredictedProjectileClientComponent> ent, ref StartCollideEvent args)
{
    if (ent.Comp.Hit)
        return;

    if (!TryComp(ent, out ProjectileComponent? projectile) ||
        !TryComp(ent, out PhysicsComponent? physics))
    {
        return;
    }

    // Skip collision with shooter and weapon if IgnoreShooter is true
    if (args.OurFixtureId != ProjectileFixture || !args.OtherFixture.Hard ||
        projectile.DamagedEntity || projectile is { Weapon: null, OnlyCollideWhenShot: true })
        return;

    // Skip puddles - they should never be hit by projectiles
    if (HasComp<PuddleComponent>(args.OtherEntity))
        return;

    // Check if contact has physics component
    if (!TryComp<PhysicsComponent>(args.OtherEntity, out var contactPhysics))
        return;

    // Check if contact is anchored for directional filtering
    var isAnchored = false;
    if (TryComp<TransformComponent>(args.OtherEntity, out var contactXform))
        isAnchored = contactXform.Anchored;

    // Additional filtering for non-anchored entities - match Update() logic
    if (!isAnchored)
    {
        // Only hit non-anchored entities if they can be damaged or are mobs
        var canBeHit = HasComp<DamageableComponent>(args.OtherEntity) ||
                       HasComp<MobStateComponent>(args.OtherEntity);

        if (!canBeHit)
            return;
    }

    // For anchored entities (walls, fixtures), check if they're in the direction of travel
    if (isAnchored && physics.LinearVelocity.LengthSquared() > 0.01f)
    {
        var projectileMapCoords = _transform.GetMapCoordinates(ent);
        var contactMapCoords = _transform.GetMapCoordinates(args.OtherEntity);
        var toContact = contactMapCoords.Position - projectileMapCoords.Position;

        var toContactNormalized = toContact.Normalized();
        var velocityNormalized = physics.LinearVelocity.Normalized();
        var dot = Vector2.Dot(toContactNormalized, velocityNormalized);

        // Only collide with anchored entities if they're in front
        if (dot < 0.3f)
            return;
    }

    var netEnt = GetNetEntity(args.OtherEntity);
    var pos = _transform.GetMapCoordinates(args.OtherEntity);
    var hit = new HashSet<(NetEntity, MapCoordinates)> { (netEnt, pos) };
    var ev = new PredictedProjectileHitEvent(ent.Owner.Id, hit);
    RaiseNetworkEvent(ev);

    _projectile.ProjectileCollide((ent, projectile, physics), args.OtherEntity, predicted: true);
}

    private void OnServerProjectileStartup(Entity<PredictedProjectileServerComponent> ent, ref ComponentStartup args)
    {
        if (!GunPrediction)
            return;

        if (ent.Comp.ClientEnt != _player.LocalEntity)
            return;

        if (_ignorePredictionHideQuery.HasComp(ent))
            return;

        if (_spriteQuery.TryComp(ent, out var sprite))
            sprite.Visible = false;
    }

public override void Update(float frameTime)
{
    base.Update(frameTime);

    if (!_timing.IsFirstTimePredicted)
        return;

    // TODO gun prediction remove this once the client reliably detects collisions
    var projectiles = EntityQueryEnumerator<PredictedProjectileClientComponent, ProjectileComponent, PhysicsComponent>();
    while (projectiles.MoveNext(out var uid, out var predicted, out var projectile, out var physics))
    {
        if (predicted.Hit)
            continue;

        var contacts = _physics.GetContactingEntities(uid, physics, true);
        if (contacts.Count == 0)
            continue;

        // Get fixtures component
        if (!TryComp<FixturesComponent>(uid, out var fixtures))
            continue;

        // Get the projectile fixture specifically
        if (!fixtures.Fixtures.TryGetValue("projectile", out var projectileFixture))
            continue;

        var projectileMask = projectileFixture.CollisionMask;

        // Get projectile position and velocity for directional checking
        var projectileMapCoords = _transform.GetMapCoordinates(uid);
        var projectileVelocity = physics.LinearVelocity;
        var hasVelocity = projectileVelocity.LengthSquared() > 0.01f;

        // Filter contacts - matching server-side logic from SharedProjectileSystem.OnStartCollide
        var filteredContacts = new List<EntityUid>();
        foreach (var contact in contacts)
        {
            // Skip shooter and weapon to prevent immediate collision at spawn point
            if (projectile.IgnoreShooter && (contact == projectile.Shooter || contact == projectile.Weapon))
                continue;

            // Skip puddles - they should never be hit by projectiles
            if (HasComp<PuddleComponent>(contact))
                continue;

            // Check if contact has physics - if not, skip it
            if (!TryComp<PhysicsComponent>(contact, out var contactPhysics))
                continue;


            // Get contact fixtures to check which fixture is actually colliding
            if (!TryComp<FixturesComponent>(contact, out var contactFixtures))
                continue;

            // Check if contact is anchored
            var isAnchored = false;
            if (TryComp<TransformComponent>(contact, out var contactXform))
                isAnchored = contactXform.Anchored;

            // Check if any of the contact's fixtures would collide with the projectile fixture
            var shouldCollide = false;

            foreach (var fixture in contactFixtures.Fixtures.Values)
            {
                // Must be a hard fixture (not a trigger/sensor)
                if (!fixture.Hard)
                    continue;

                // Must have collision layer overlap with projectile's mask
                if ((fixture.CollisionLayer & projectileMask) == 0)
                    continue;

                shouldCollide = true;
                break;
            }

            if (!shouldCollide)
                continue;

            // Additional component-based filtering for non-anchored entities
            if (!isAnchored)
            {
                // Only hit non-anchored entities if they can be damaged or are mobs
                var canBeHit = HasComp<DamageableComponent>(contact) ||
                               HasComp<MobStateComponent>(contact);

                if (!canBeHit)
                    continue;
            }

            // For anchored entities (walls, fixtures), check if they're in the direction of travel
            // This prevents hitting walls behind the shooter
            if (hasVelocity && isAnchored)
            {
                var contactMapCoords = _transform.GetMapCoordinates(contact);
                var toContact = contactMapCoords.Position - projectileMapCoords.Position;

                // Calculate dot product to check if contact is in front of projectile
                var toContactNormalized = toContact.Normalized();
                var velocityNormalized = projectileVelocity.Normalized();
                var dot = Vector2.Dot(toContactNormalized, velocityNormalized);

                // Only collide with anchored entities if they're in front
                if (dot < 0.3f)
                    continue;
            }

            filteredContacts.Add(contact);
        }

        if (filteredContacts.Count == 0)
            continue;

        var hit = new HashSet<(NetEntity, MapCoordinates)>();
        foreach (var contact in filteredContacts)
        {
            var netEnt = GetNetEntity(contact);
            var pos = _transform.GetMapCoordinates(contact);
            hit.Add((netEnt, pos));
        }

        var ev = new PredictedProjectileHitEvent(uid.Id, hit);
        RaiseNetworkEvent(ev);

        _projectile.ProjectileCollide((uid, projectile, physics), filteredContacts.First());
    }

    var predictedQuery = EntityQueryEnumerator<PredictedProjectileHitComponent, SpriteComponent, TransformComponent>();
    while (predictedQuery.MoveNext(out var hit, out var sprite, out var xform))
    {
        var origin = hit.Origin;
        var coordinates = xform.Coordinates;
        if (!origin.TryDistance(EntityManager, _transform, coordinates, out var distance) ||
            distance >= hit.Distance)
        {
            sprite.Visible = false;
        }
    }
}

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        // TODO bullet prediction remove this when lerping doesnt make the client's entity slightly slower
        var projectiles = EntityQueryEnumerator<PredictedProjectileClientComponent, TransformComponent>();
        while (projectiles.MoveNext(out _, out var xform))
        {
            xform.ActivelyLerping = false;
        }
    }
}
