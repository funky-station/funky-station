// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client.Movement.Systems;

/// <summary>
/// Handles setting sprite states based on whether an entity has movement input.
/// </summary>
public sealed class SpriteMovementSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpriteMovementComponent, MoveInputEvent>(OnSpriteMoveInput);
        _spriteQuery = GetEntityQuery<SpriteComponent>();
    }

    private void OnSpriteMoveInput(EntityUid uid, SpriteMovementComponent component, ref MoveInputEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var oldMoving = (SharedMoverController.GetNormalizedMovement(args.OldMovement) & MoveButtons.AnyDirection) != MoveButtons.None;
        var moving = (SharedMoverController.GetNormalizedMovement(args.Entity.Comp.HeldMoveButtons) & MoveButtons.AnyDirection) != MoveButtons.None;

        var oldWalking = (SharedMoverController.GetNormalizedMovement(args.OldMovement) & MoveButtons.Walk) != MoveButtons.None;
        var walking = (SharedMoverController.GetNormalizedMovement(args.Entity.Comp.HeldMoveButtons) & MoveButtons.Walk) != MoveButtons.None;

        if ((oldMoving == moving && oldWalking == walking) || !_spriteQuery.TryGetComponent(uid, out var sprite))
            return;

        if (moving)
        {
            foreach (var (layer, state) in component.MovementLayers)
            {
                sprite.LayerSetData(layer, state);
            }

            if (walking)
            {
                foreach (var (layer, state) in component.WalkLayers)
                {
                    sprite.LayerSetData(layer, state);
                }
            }
            else
            {
                foreach (var (layer, state) in component.RunLayers)
                {
                    sprite.LayerSetData(layer, state);
                }
            }
        }
        else
        {
            foreach (var (layer, state) in component.NoMovementLayers)
            {
                sprite.LayerSetData(layer, state);
            }
        }
    }
}
