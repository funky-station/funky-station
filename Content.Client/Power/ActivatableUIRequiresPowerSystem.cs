// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Power.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.Wires;

namespace Content.Client.Power;

public sealed class ActivatableUIRequiresPowerSystem : SharedActivatableUIRequiresPowerSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    protected override void OnActivate(Entity<ActivatableUIRequiresPowerComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled || this.IsPowered(ent.Owner, EntityManager))
        {
            return;
        }

        _popup.PopupClient(Loc.GetString("base-computer-ui-component-not-powered", ("machine", ent.Owner)), args.User, args.User);
        args.Cancel();
    }
}
