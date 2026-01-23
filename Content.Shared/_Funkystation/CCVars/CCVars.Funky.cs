// SPDX-FileCopyrightText: 2025 duston <66768086+dch-GH@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2026 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVars;

[CVarDefs]
public sealed class CCVars_Funky
{
    #region Secret Director

    public static readonly CVarDef<string> DirectorWeightPrototype =
        CVarDef.Create("game.director_weight_prototype", "SecretDirector", CVar.SERVERONLY);

    #endregion

    /// <summary>
    ///     Is bluespace gas enabled.
    /// </summary>
    public static readonly CVarDef<bool> BluespaceGasEnabled =
        CVarDef.Create("funky.bluespace_gas_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// If the LOOC message budget system should be enabled.
    /// Defaults to true.
    /// </summary>
    public static readonly CVarDef<bool> LoocBudgetEnabled =
        CVarDef.Create("funky.looc_budget_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// How many LOOC messages each player can send per round.
    /// Defaults to 20.
    /// </summary>
    public static readonly CVarDef<int> DefaultLoocBudget =
        CVarDef.Create("funky.looc_budget_default", 20, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     If true, gas extractors linked to a console require payment/budget (deduct RemainingMoles, allow auto-buy).
    ///     If false, linked extractors behave like normal miners (spawn full amount regardless of budget).
    ///     Defaults to false while testing.
    /// </summary>
    public static readonly CVarDef<bool> GasExtractorsRequirePayment =
        CVarDef.Create("funky.gas_extractors_require_payment", false, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// If objectives show up as completed or failed (green or red) on the round end summary.
    /// </summary>
    public static readonly CVarDef<bool> GreentextEnabled =
        CVarDef.Create("objectives.green_text_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// If custom objective summaries are enabled.
    /// </summary>
    public static readonly CVarDef<bool> PinktextEnabled =
        CVarDef.Create("objectives.pink_text_enabled", true, CVar.SERVER | CVar.REPLICATED);
}
