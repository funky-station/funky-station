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

public sealed class HLine : Container
{
    public Color? Color
    {
        get
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) return styleBox.BackgroundColor;
            return null;
        }
        set
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) styleBox.BackgroundColor = value!.Value;
        }
    }

    public float? Thickness {
        get
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) return styleBox.ContentMarginTopOverride;
            return null;
        }
        set
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) styleBox.ContentMarginTopOverride = value!.Value;
        }
    }

    private readonly PanelContainer _line;

    public HLine()
    {
        _line = new PanelContainer();
        _line.PanelOverride = new StyleBoxFlat();
        _line.PanelOverride.ContentMarginTopOverride = Thickness;
        AddChild(_line);
    }

}
