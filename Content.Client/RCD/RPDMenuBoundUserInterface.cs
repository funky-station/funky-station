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
using Content.Client.UserInterface.Controls;
using Robust.Shared.Collections;
using System.Collections.Generic;

namespace Content.Client.RCD;

public sealed class RPDMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private SimpleRadialMenu? _menu;
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

        // Create and track the SimpleRadialMenu (matches RCDMenuBoundUserInterface pattern).
        _menu = new SimpleRadialMenu();
        _menu.Track(Owner);

        // Build models: a nested layer for prototypes, and individual color options.
        var models = BuildModels();

        _menu.SetButtons(models);

        // Open at the mouse position
        _menu.OpenOverMouseScreenPosition();
    }

    private RadialMenuOptionBase[] BuildModels()
    {
        // 1) Build prototype actions into a nested layer
        var protoOptions = new List<RadialMenuActionOptionBase>();
        if (_entityManager.TryGetComponent<RCDComponent>(Owner, out var comp))
        {
            foreach (var protoId in comp.AvailablePrototypes)
            {
                // Safely index prototype
                if (!_prototypeManager.TryIndex(protoId, out RCDPrototype? proto))
                    continue;

                var action = new RadialMenuActionOption<ProtoId<RCDPrototype>>(OnRCDSystemMessage, protoId)
                {
                    IconSpecifier = RadialMenuIconSpecifier.With(proto.Sprite),
                    ToolTip = proto.SetName
                };

                protoOptions.Add(action);
            }
        }

        // Create a nested layer only if there are prototypes.
        var modelsList = new List<RadialMenuOptionBase>();

        if (protoOptions.Count > 0)
        {
            var nested = new RadialMenuNestedLayerOption(protoOptions)
            {
                ToolTip = Loc.GetString("rcd-components"),
                // No specific icon supplied here; engine will show a default or none.
            };
            modelsList.Add(nested);
        }

        // 2) Build color action options (flat, top-level)
        foreach (var kv in _palette)
        {
            var key = kv.Key;
            var color = kv.Value;

            // When clicked, send the same payload you used previously.
            var action = new RadialMenuActionOption<string>(OnColorSelected, key)
            {
                ToolTip = Loc.GetString($"rcd-color-{key}"),
                // Optionally set an icon that visually represents the color if you have a sprite.
                // We'll skip an icon here; if you want a colored sprite, provide a SpriteSpecifier.
            };

            modelsList.Add(action);
        }

        return modelsList.ToArray();
    }

    private void OnColorSelected(string colorKey)
    {
        if (!_palette.TryGetValue(colorKey, out var color))
            return;

        var pipeColor = (colorKey, color);
        SendMessage(new RCDColorChangeMessage(_entityManager.GetNetEntity(Owner), pipeColor));
    }

    private void OnRCDSystemMessage(ProtoId<RCDPrototype> protoId)
    {
        SendMessage(new RCDSystemMessage(protoId));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _menu?.Dispose();
        }

        base.Dispose(disposing);
    }
}
