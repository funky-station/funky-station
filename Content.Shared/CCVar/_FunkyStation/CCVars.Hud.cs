// SPDX-FileCopyrightText: 2025 Ekpy <33184056+Ekpy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

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
