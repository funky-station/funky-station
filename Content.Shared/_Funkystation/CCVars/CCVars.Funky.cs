using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVars;

/// <summary>
/// _DV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed class FSCCVars
{
    /// <summary>
    /// Allow forks to save a persistent balance for a character. Works kinda like Frontier (shout out frontier).
    /// </summary>
    public static readonly CVarDef<bool> EnablePersistentBalance =
        CVarDef.Create("banking.enable_persistent_balance", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
