// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 Tojo <32783144+Alecksohs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Stylesheets;

///
/// <summary>
///     A collection of public reusable style classes. These should be general purpose (Not specific to only one element
///     or Ui).
/// </summary>
/// <remarks>
///     It is named `StyleClass` as opposed to `StyleClasses` because `StyleClasses` is a field on `Control` so it made
///     it a pain to reference this class from a `Control`. (Weird name is worth typing `StyleClass.OpenBoth` vs.
///     `Stylesheets.Styleclasses.OpenBoth`)
/// </remarks>
public static class StyleClass
{
    // These style classes affect more than one type of element
    public const string Positive = "positive";
    public const string Negative = "negative";
    public const string Highlight = "highlight";

    public const string StatusGood = "status-good"; //          Status.GetStatusColor(1.0f)
    public const string StatusOkay = "status-okay"; //          Status.GetStatusColor(0.75f)
    public const string StatusWarning = "status-warning"; //    Status.GetStatusColor(0.5f)
    public const string StatusBad = "status-bad"; //            Status.GetStatusColor(0.25f)
    public const string StatusCritical = "status-critical"; //  Status.GetStatusColor(0.0f)

    public const string FontLarge = "font-large";
    public const string FontCondensed = "font-condensed";
    public const string FontSmall = "font-small";
    public const string Italic = "italic";
    public const string Monospace = "monospace";

    public const string BorderedWindowPanel = "BorderedWindowPanel";
    public const string AlertWindowHeader = "windowHeaderAlert";
    public const string WindowContentsContainer = "WindowContentsContainer";

    public const string HighDivider = "HighDivider";
    public const string LowDivider = "LowDivider";

    public const string LabelHeading = "LabelHeading";
    public const string LabelHeadingBigger = "LabelHeadingBigger";
    public const string LabelSubText = "LabelSubText";
    public const string LabelKeyText = "LabelKeyText";
    public const string LabelWeak = "LabelWeak"; // replaces `LabelSecondaryColor`
    public const string LabelMonospaceText = "ConsoleText";
    public const string LabelMonospaceHeading = "ConsoleText";
    public const string LabelMonospaceSubHeading = "ConsoleText";

    public const string BackgroundPanel = "BackgroundPanel"; // replaces `AngleRect`
    public const string BackgroundPanelOpenLeft = "BackgroundPanelOpenLeft"; // replaces `BackgroundOpenLeft`
    public const string BackgroundPanelOpenRight = "BackgroundPanelOpenRight"; // replaces `BackgroundOpenRight`

    public const string PanelDark = "PanelDark";
    public const string PanelLight = "PanelLight";

    public const string ButtonOpenRight = "OpenRight";
    public const string ButtonOpenLeft = "OpenLeft";
    public const string ButtonOpenBoth = "OpenBoth";
    public const string ButtonSquare = "ButtonSquare";
    public const string ButtonSmall = "ButtonSmall";
    public const string ButtonBig = "ButtonBig";

    public const string CrossButtonRed = "CrossButtonRed";

    public const string ItemStatus = "ItemStatus";
    public const string ItemStatusNotHeld = "ItemStatusNotHeld";

    public const string TooltipPanel = "TooltipPanel";
    public const string TooltipTitle = "TooltipTitle";
    public const string TooltipDesc = "TooltipDesc";


    public const string BorderModal = "BorderModal";

}
