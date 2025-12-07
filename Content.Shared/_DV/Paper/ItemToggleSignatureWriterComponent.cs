// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._DV.Paper;

[RegisterComponent]
public sealed partial class ItemToggleSignatureWriterComponent : Component
{
    /// <summary>
    /// The font used when this item is activated.
    /// </summary>
    [DataField("activatedFont")]
    public string? ActivatedFont;

    /// <summary>
    /// The list of fonts used when this item is activated.
    /// </summary>
    [DataField("activatedFontList")]
    public Dictionary<string, string> ActivatedFontList = new();

    /// <summary>
    /// The color used when this item is activated.
    /// </summary>
    [DataField("activatedColor")]
    public Color? ActivatedColor;

    /// <summary>
    /// The list of colors used when this item is activated.
    /// </summary>
    [DataField("activatedColorList")]
    public Dictionary<string, Color> ActivatedColorList = new();

    /// <summary>
    /// The font used when this item is deactivated.
    /// </summary>
    [DataField("deactivatedFont")]
    public string? DeactivatedFont;

    /// <summary>
    /// The list of fonts used when this item is deactivated.
    /// </summary>
    [DataField("deactivatedFontList")]
    public Dictionary<string, string> DeactivatedFontList = new();

    /// <summary>
    /// The color used when this item is deactivated.
    /// </summary>
    [DataField("deactivatedColor")]
    public Color? DeactivatedColor;

    /// <summary>
    /// The list of colors used when this item is deactivated.
    /// </summary>
    [DataField("deactivatedColorList")]
    public Dictionary<string, Color> DeactivatedColorList = new();
}
