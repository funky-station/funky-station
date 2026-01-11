// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Input;
using System.Numerics;

namespace Content.Client._Funkystation.Genetics.DnaScannerConsole.UI;

public sealed partial class UniqueEnzymeButton : Button
{
    public int Index { get; set; }
    private readonly EntityPrototypeView _iconView;
    public float IconScale { get; set; } = 1f;

    private const string DiscoveredIconPrototype = "DnaDiscoveredIcon";
    private const string UndiscoveredIconPrototype = "DnaUndiscoveredIcon";
    private const string ExtraIconPrototype = "DnaExtraIcon";

    public UniqueEnzymeButton()
    {
        RobustXamlLoader.Load(this);

        _iconView = new EntityPrototypeView
        {
            MinSize = new Vector2(61, 35),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center
        };

        AddChild(_iconView);
        _iconView.SetPrototype(DiscoveredIconPrototype);
        _iconView.Scale = new Vector2(IconScale, IconScale);
    }

    public void UpdateIcon(bool isDiscovered, bool isBase)
    {
        string prototype;
        if (!isDiscovered)
        {
            prototype = UndiscoveredIconPrototype;
        }
        else if (isBase)
        {
            prototype = DiscoveredIconPrototype;
        }
        else
        {
            prototype = ExtraIconPrototype;
        }

        _iconView.SetPrototype(prototype);
    }
}
