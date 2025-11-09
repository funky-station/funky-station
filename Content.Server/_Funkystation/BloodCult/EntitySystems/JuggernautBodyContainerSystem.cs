// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Server.Mind;
using Content.Shared.BloodCult.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class JuggernautBodyContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<JuggernautBodyContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, JuggernautBodyContainerComponent component, MobStateChangedEvent args)
    {
        // When the juggernaut goes critical or dies, eject the body
        if (args.NewMobState == MobState.Critical || args.NewMobState == MobState.Dead)
        {
            EjectBody(uid, component);
        }
    }

    private void EjectBody(EntityUid uid, JuggernautBodyContainerComponent component)
    {
        if (!_container.TryGetContainer(uid, component.ContainerId, out var container))
            return;

        var coordinates = Transform(uid).Coordinates;
        
        // Get the juggernaut's mind before ejecting
        EntityUid? juggernautMindId = CompOrNull<MindContainerComponent>(uid)?.Mind;
        MindComponent? juggernautMindComp = CompOrNull<MindComponent>(juggernautMindId);
        
        // Eject all entities from the container (should just be the body)
        foreach (var contained in container.ContainedEntities.ToArray())
        {
            _container.Remove(contained, container, destination: coordinates);
            
            // Give the body a physics push for visual effect
            if (TryComp<PhysicsComponent>(contained, out var physics))
            {
                // Wake the physics body so it responds to the impulse
                _physics.SetAwake((contained, physics), true);
                
                // Generate a random direction and speed (8-15 units/sec for dramatic ejection)
                var randomDirection = _random.NextVector2();
                var speed = _random.NextFloat(8f, 15f);
                var impulse = randomDirection * speed * physics.Mass;
                _physics.ApplyLinearImpulse(contained, impulse, body: physics);
            }
            
            // Transfer the mind back to the body
            if (juggernautMindId != null && juggernautMindComp != null)
            {
                _mind.TransferTo((EntityUid)juggernautMindId, contained, mind: juggernautMindComp);
            }
        }
    }
}

