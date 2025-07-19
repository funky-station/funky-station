// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Duration for missions
    /// </summary>
    public static readonly CVarDef<float>
        SalvageExpeditionDuration = CVarDef.Create("salvage.expedition_duration", 660f, CVar.REPLICATED);

    /// <summary>
    ///     Cooldown for missions.
    /// </summary>
    public static readonly CVarDef<float>
        SalvageExpeditionCooldown = CVarDef.Create("salvage.expedition_cooldown", 780f, CVar.REPLICATED);
}
