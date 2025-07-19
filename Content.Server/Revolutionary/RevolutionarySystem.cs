// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Shared.Revolutionary;
using Content.Shared.Revolutionary.Components;


namespace Content.Server.Revolutionary;

public sealed class RevolutionarySystem : SharedRevolutionarySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadRevolutionaryComponent, ComponentInit>(OnStartHeadRev);
    }

    /// <summary>
    /// Add the starting ability(s) to the Head Rev.
    /// </summary>
    private void OnStartHeadRev(Entity<HeadRevolutionaryComponent> uid, ref ComponentInit args)
    {
        foreach (var actionId in uid.Comp.BaseHeadRevActions)
        {
            var actionEnt = _actions.AddAction(uid, actionId);
        }
    }
}
