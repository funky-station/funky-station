// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared;
using Robust.Shared.Prototypes;

namespace Content.Client.RCD;

public sealed class RPDMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private RPDMenu? _menu;
    private Dictionary<string, Color?> _palette = new()
    {
        { "default", null },
        { "red", Color.FromHex("#FF1212FF") },
        { "yellow", Color.FromHex("#B3A234FF") },
        { "brown", Color.FromHex("#947507FF") },
        { "green", Color.FromHex("#3AB334FF") },
        { "cyan", Color.FromHex("#03FCD3FF") },
        { "blue", Color.FromHex("#0335FCFF") },
        { "white", Color.FromHex("#FFFFFFFF") },
        { "black", Color.FromHex("#333333FF") },
        { "waste", Color.FromHex("#990000") },
        { "distro", Color.FromHex("#0055cc") },
        { "air", Color.FromHex("#03fcd3") },
        { "mix", Color.FromHex("#947507") }
    };

    public RPDMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (!_entityManager.HasComponent<RCDComponent>(Owner))
            return;

        _menu = this.CreateWindow<RPDMenu>();
        _menu.SetEntity(Owner);
        _menu.ColorSelected += OnColorSelected;
        _menu.SendRCDSystemMessageAction += OnRCDSystemMessage;

        string selectedColor = _entityManager.TryGetComponent<RCDComponent>(Owner, out var comp) && _palette.ContainsKey(comp.PipeColor.Key)
            ? comp.PipeColor.Key
            : "default";
        _menu.Populate(_palette, selectedColor);

        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    private void OnColorSelected(string colorKey)
    {
        if (_palette.TryGetValue(colorKey, out var color))
        {
            var pipeColor = (colorKey, color);
            SendMessage(new RCDColorChangeMessage(_entityManager.GetNetEntity(Owner), pipeColor));
        }
    }

    private void OnRCDSystemMessage(ProtoId<RCDPrototype> protoId)
    {
        SendMessage(new RCDSystemMessage(protoId));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_menu != null)
            {
                _menu.ColorSelected -= OnColorSelected;
                _menu.SendRCDSystemMessageAction -= OnRCDSystemMessage;
            }
        }
        base.Dispose(disposing);
    }
}