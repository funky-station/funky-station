// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 Tojo <32783144+Alecksohs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Resources;
using Content.Client.Stylesheets.Fonts;
using Robust.Client.UserInterface;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

/// These are not in `LabelSheetlet` because a label is not the only thing you might want to be monospaced.
[CommonSheetlet]
public sealed class TextSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // TODO: once fonts are reworked, change this!
        var mono = ResCache.GetFont("/EngineFonts/NotoSans/NotoSansMono-Regular.ttf", 12);

        return
        [
            E().Class(StyleClass.Monospace).Font(mono),
            E().Class(StyleClass.Italic).Font(sheet.BaseFont.GetFont(12, FontKind.Italic)),
            E().Class(StyleClass.FontLarge).Font(sheet.BaseFont.GetFont(14)),
            E().Class(StyleClass.FontCondensed).Font(sheet.BaseFont.GetFont(11)),
            E().Class(StyleClass.FontSmall).Font(sheet.BaseFont.GetFont(10)),
        ];
    }
}
