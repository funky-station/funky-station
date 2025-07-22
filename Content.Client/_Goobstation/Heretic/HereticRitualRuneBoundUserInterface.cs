// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._Goobstation.Heretic.UI;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Heretic;

public sealed class HereticRitualRuneBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private HereticRitualRuneRadialMenu? _hereticRitualMenu;

    public HereticRitualRuneBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _hereticRitualMenu = this.CreateWindow<HereticRitualRuneRadialMenu>();
        _hereticRitualMenu.SetEntity(Owner);
        _hereticRitualMenu.SendHereticRitualRuneMessageAction += SendHereticRitualMessage;

        var vpSize = _displayManager.ScreenSize;
        _hereticRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    private void SendHereticRitualMessage(ProtoId<HereticRitualPrototype> protoId)
    {
        SendMessage(new HereticRitualMessage(protoId));
    }
}
