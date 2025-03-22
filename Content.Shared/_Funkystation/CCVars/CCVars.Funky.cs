using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVars;

[CVarDefs]
public sealed class CCVars_Funky
{
    #region Secret Director

    public static readonly CVarDef<string> DirectorWeightPrototype =
        CVarDef.Create("game.director_weight_prototype", "SecretDirector", CVar.SERVERONLY);

    #endregion
}
