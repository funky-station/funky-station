using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CVars;

[CVarDefs]
public sealed class GoobCVars : Robust.Shared.CVars
{
    /// <summary>
    ///     Is ore silo enabled.
    /// </summary>
    public static readonly CVarDef<bool> SiloEnabled =
        CVarDef.Create("goob.silo_enabled", true, CVar.SERVER | CVar.REPLICATED);
}
