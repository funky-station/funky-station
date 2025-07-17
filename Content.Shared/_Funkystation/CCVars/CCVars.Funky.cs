// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
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
}
