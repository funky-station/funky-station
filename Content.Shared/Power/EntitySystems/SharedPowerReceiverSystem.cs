// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Power.Components;

namespace Content.Shared.Power.EntitySystems;

public abstract class SharedPowerReceiverSystem : EntitySystem
{
    public abstract bool ResolveApc(EntityUid entity, [NotNullWhen(true)] ref SharedApcPowerReceiverComponent? component);

    /// <summary>
    /// Checks if entity is APC-powered device, and if it have power.
    /// </summary>
    public bool IsPowered(Entity<SharedApcPowerReceiverComponent?> entity)
    {
        if (!ResolveApc(entity.Owner, ref entity.Comp))
            return true;

        return entity.Comp.Powered;
    }

    protected string GetExamineText(bool powered)
    {
        return Loc.GetString("power-receiver-component-on-examine-main",
                                ("stateText", Loc.GetString(powered
                                    ? "power-receiver-component-on-examine-powered"
                                    : "power-receiver-component-on-examine-unpowered")));
    }
}
