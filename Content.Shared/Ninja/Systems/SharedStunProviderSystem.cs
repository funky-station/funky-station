// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Ninja.Components;

namespace Content.Shared.Ninja.Systems;

/// <summary>
/// All interaction logic is implemented serverside.
/// This is in shared for API and access.
/// </summary>
public abstract class SharedStunProviderSystem : EntitySystem
{
    /// <summary>
    /// Set the battery field on the stun provider.
    /// </summary>
    public void SetBattery(Entity<StunProviderComponent?> ent, EntityUid? battery)
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.BatteryUid == battery)
            return;

        ent.Comp.BatteryUid = battery;
        Dirty(ent, ent.Comp);
    }
}
