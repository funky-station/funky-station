// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.CartridgeLoader.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class LogProbeUiEntry : BoxContainer
{
    public LogProbeUiEntry(int numberLabel, string timeText, string accessorText)
    {
        RobustXamlLoader.Load(this);
        NumberLabel.Text = numberLabel.ToString();
        TimeLabel.Text = timeText;
        AccessorLabel.Text = accessorText;
    }
}
