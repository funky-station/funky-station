using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///    Deciding crayon transparentcy for CrayonPlacementOverlay
    /// </summary>
    public static readonly CVarDef<float> CrayonOverlayTransparency =
        CVarDef.Create("hud.crayon_overlay_transparency", 0.75f, CVar.ARCHIVE | CVar.CLIENTONLY);
}
