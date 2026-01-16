// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Genetics.GeneticAnalyzer;

[UsedImplicitly]
public sealed class GeneticAnalyzerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GeneticAnalyzerWindow? _window;

    public GeneticAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GeneticAnalyzerWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

        _window.OnPrintPressed += () => SendMessage(new GeneticAnalyzerPrintMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GeneticAnalyzerUiState uiState)
            return;

        _window?.Populate(uiState);
    }
}
