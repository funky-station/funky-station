// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Log;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.Prototypes;
using Content.Client._Funkystation.BloodCult.UI;
using Robust.Shared.Timing;

namespace Content.Client._Funkystation.BloodCult;

public sealed class SpellsBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _log = default!;
    private SpellRadialMenu? _spellRitualMenu;

    public SpellsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _log = _logManager.GetSawmill("SpellsBoundUserInterface");
    }

    protected override void Open()
    {
        base.Open();

        // If window already exists and is open, just bring it to front
        if (_spellRitualMenu != null && _spellRitualMenu.IsOpen)
        {
            _spellRitualMenu.MoveToFront();
            return;
        }

        _spellRitualMenu = this.CreateWindow<SpellRadialMenu>();
        if (_spellRitualMenu != null)
        {
            _spellRitualMenu._shouldRefresh = true;
            _spellRitualMenu.SetEntity(Owner);
            _spellRitualMenu.SendSpellsMessageAction += SendSpellsMessage;
        }

        if (_spellRitualMenu != null)
        {
            var vpSize = _displayManager.ScreenSize;
            _spellRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_spellRitualMenu != null)
        {
            _spellRitualMenu.Close();
            _spellRitualMenu = null;
        }
    }

    private void SendSpellsMessage(ProtoId<CultAbilityPrototype> protoId)
    {
        // A predicted message cannot be used here as the spell selection UI is closed immediately
        // after this message is sent, which will stop the server from receiving it
        // _log.Info($"[SpellsBoundUserInterface] SendSpellsMessage called with spell {protoId}, IsOpened={IsOpened}, Owner={Owner}");
        SendMessage(new SpellsMessage(protoId));
        // _log.Info($"[SpellsBoundUserInterface] SendMessage completed for spell {protoId}");
    }
}
