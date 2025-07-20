// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;

namespace Content.Shared.Movement.Systems;

public abstract class SharedSpriteMovementSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpriteMovementComponent, SpriteMoveEvent>(OnSpriteMoveInput);
    }

    private void OnSpriteMoveInput(Entity<SpriteMovementComponent> ent, ref SpriteMoveEvent args)
    {
        if (ent.Comp.IsMoving == args.IsMoving)
            return;

        ent.Comp.IsMoving = args.IsMoving;
        Dirty(ent);
    }
}
