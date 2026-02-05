// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client._Funkystation.Genetics.GeneticistsConsole.UI;

public sealed class SequencerButton : Button
{
    public int Index { get; set; }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        if (args.Function != EngineKeyFunctions.UIClick && args.Function != EngineKeyFunctions.UIRightClick)
        {
            base.KeyBindDown(args);
            return;
        }

        if (Disabled)
        {
            args.Handle();
            return;
        }

        if (!MuteSounds)
            UserInterfaceManager.ClickSound();

        DrawModeChanged();

        var current = this.Parent;
        GeneticistsConsoleUniqueEnzymesView? window = null;
        while (current != null)
        {
            if (current is GeneticistsConsoleUniqueEnzymesView w)
            {
                window = w;
                break;
            }
            current = current.Parent;
        }

        if (window != null)
        {
            bool reverse = args.Function == EngineKeyFunctions.UIRightClick;
            window.CycleBase(Index, reverse);
        }

        args.Handle();
        return;
    }
}
