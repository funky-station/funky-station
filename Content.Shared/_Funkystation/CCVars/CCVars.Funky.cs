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
