using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.VendingMachines.UI;

[CommonSheetlet]
public sealed class VendingMachineMenuSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        IWindowConfig windowCfg = sheet;

        // get any textures / construct any complicated resources here
        var paperBackground = ResCache.GetTexture("/Textures/Interface/Paper/paper_background_default.svg.96dpi.png")
            .IntoPatch(StyleBox.Margin.All, 16);
        var paperBox = new StyleBoxTexture
            { Texture = sheet.GetTexture(windowCfg.TransparentWindowBackgroundBorderedPath) };
        paperBox.SetPatchMargin(StyleBox.Margin.All, 2);

        // and finally, define all the style rules and return a big 'ol list of them
        return
        [
            E<PanelContainer>().Identifier("VendingEntryButton").Panel(paperBox),
            E<PanelContainer>()
                .Identifier("PaperDefaultBorder")
                .Prop(PanelContainer.StylePropertyPanel, paperBackground),
        ];
    }
}
