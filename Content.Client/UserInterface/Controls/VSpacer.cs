// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Controls;

public sealed class VSpacer : Control
{
    public float Spacing{ get => MinWidth; set => MinWidth = value; }
    public VSpacer()
    {
        MinWidth = Spacing;
    }
    public VSpacer(float width = 5)
    {
        Spacing = width;
        MinWidth = width;
    }
}
