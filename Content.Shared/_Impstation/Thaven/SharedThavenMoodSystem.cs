// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Emag.Systems;
using Content.Shared._Impstation.Thaven.Components;

namespace Content.Shared._Impstation.Thaven;

public abstract class SharedThavenMoodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThavenMoodsBoundComponent, GotEmaggedEvent>(OnEmagged);
    }
    protected virtual void OnEmagged(EntityUid uid, ThavenMoodsBoundComponent comp, ref GotEmaggedEvent args)
    {
        args.Handled = true;
    }
}
