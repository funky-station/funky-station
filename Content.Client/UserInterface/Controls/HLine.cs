// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Controls;

public sealed class HLine : PanelContainer
{
    public Color? Color
    {
        get
        {
            StyleBox box;
            if (PanelOverride != null)
                box = PanelOverride;
            if (TryGetStyleProperty<StyleBox>(StylePropertyPanel, out var _box))
                box = _box;
            else
                return null;

            if (box is StyleBoxFlat boxFlat)
                return boxFlat.BackgroundColor;
            return null;
        }
        set =>
            // should use style classes instead in ui code but keeping this functionality for consistency
            PanelOverride = new StyleBoxFlat() { BackgroundColor = value!.Value };
    }

    public float? Thickness
    {
        get => MinHeight;
        set => MinHeight = value!.Value;
    }

    public HLine()
    {
    }
}
