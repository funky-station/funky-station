// SPDX-FileCopyrightText: 2024 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mind.Components;

namespace Content.Shared.Mind;

/// <summary>
/// This marks any entity with the component as dead
/// for stuff like objectives & round-end
/// used for nymphs & reformed diona.
/// </summary>
public sealed class IsDeadICSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<IsDeadICComponent, GetCharactedDeadIcEvent>(OnGetDeadIC);
    }

    private void OnGetDeadIC(EntityUid uid, IsDeadICComponent component, ref GetCharactedDeadIcEvent args)
    {
        args.Dead = true;
    }
}
