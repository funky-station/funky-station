// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool>
        OocEnabled = CVarDef.Create("ooc.enabled", true, CVar.NOTIFY | CVar.REPLICATED);

    public static readonly CVarDef<bool> AdminOocEnabled =
        CVarDef.Create("ooc.enabled_admin", true, CVar.NOTIFY);

    /// <summary>
    ///     If true, whenever OOC is disabled the Discord OOC relay will also be disabled.
    /// </summary>
    public static readonly CVarDef<bool> DisablingOOCDisablesRelay =
        CVarDef.Create("ooc.disabling_ooc_disables_relay", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether or not OOC chat should be enabled during a round.
    /// </summary>
    public static readonly CVarDef<bool> OocEnableDuringRound =
        CVarDef.Create("ooc.enable_during_round", false, CVar.NOTIFY | CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<bool> ShowOocPatronColor =
        CVarDef.Create("ooc.show_ooc_patron_color", true, CVar.ARCHIVE | CVar.REPLICATED | CVar.CLIENT);
}
