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
    /// Allow forks to save a persistent balance for a character. Works kinda like Frontier (shout out frontier).
    /// </summary>
    public static readonly CVarDef<bool> EnablePersistentBalance =
        CVarDef.Create("banking.enable_persistent_balance", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
