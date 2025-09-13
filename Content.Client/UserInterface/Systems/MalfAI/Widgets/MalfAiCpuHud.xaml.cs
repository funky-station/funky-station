// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Systems.MalfAI.Widgets
{
    public sealed partial class MalfAiCpuHud : Control
    {
        public MalfAiCpuHud()
        {
            RobustXamlLoader.Load(this);
        }
    }
}
