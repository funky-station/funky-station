// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Actions;

namespace Content.Server.Actions;

public sealed partial class ActionsProviderSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsProviderComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<ActionsProviderComponent> ent, ref ComponentInit args)
    {
        foreach (var action in ent.Comp.Actions)
            _actions.AddAction(ent, action);
    }
}
