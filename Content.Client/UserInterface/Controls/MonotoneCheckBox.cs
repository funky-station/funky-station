// SPDX-FileCopyrightText: 2025 Ichaie <ichaicoelho@gmail.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls;

/// <summary>
/// A check box intended for use with a monotone color palette
/// </summary>
public sealed class MonotoneCheckBox : CheckBox
{
    public const string StyleClassMonotoneCheckBox = "monotoneCheckBox";

    public MonotoneCheckBox()
    {
        TextureRect.AddStyleClass(StyleClassMonotoneCheckBox);
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();

        // Appearance modulations
        Modulate = Disabled ? Color.Gray : Color.White;
    }
}
