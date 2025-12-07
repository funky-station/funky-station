// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server._Goobstation.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        if (!TryComp<TransformComponent>(entity, out var xform))
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
        args.Handled = true;
    }
}
