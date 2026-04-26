// SPDX-FileCopyrightText: 2025 AirFryerBuyOneGetOneFree <airfryerbuyonegetonefree@gmail.com>
// SPDX-FileCopyrightText: 2025 beck <163376292+widgetbeck@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 AirFryerBuyOneGetOneFree <jakoblondon01@gmail.com>
// SPDX-FileCopyrightText: 2026 w.xyz() <84605679+pirakaplant@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.AACTablet;
using Content.Shared._DV.QuickPhrase;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.AACTablet.UI;

public sealed class AACBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private AACWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = new AACWindow(Owner);
        _window.OpenCentered();
        _window.OnClose += Close;
        _window.PhraseButtonPressed += OnPhraseButtonPressed;
    }

    private void OnPhraseButtonPressed(List<ProtoId<QuickPhrasePrototype>> phraseId)
    {
        SendMessage(new AACTabletSendPhraseMessage(phraseId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _window?.Parent?.RemoveChild(_window);
    }
}
