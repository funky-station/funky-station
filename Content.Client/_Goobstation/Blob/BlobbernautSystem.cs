// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Client.DamageState;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Client._Goobstation.Blob;

public sealed class BlobbernautSystem : SharedBlobbernautSystem
{

}

public sealed class BlobbernautVisualizerSystem : VisualizerSystem<BlobbernautComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobbernautComponent, AfterAutoHandleStateEvent>(OnBlobTileHandleState);
    }

    private static readonly DamageStateVisualLayers[] Layers =
    [
        DamageStateVisualLayers.Base, DamageStateVisualLayers.BaseUnshaded,
    ];

    private void UpdateAppearance(EntityUid id, BlobbernautComponent blobbernaut, AppearanceComponent? appearance = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(id, ref appearance, ref sprite))
            return;

        foreach (var key in Layers)
        {
            if (!sprite.LayerMapTryGet(key, out _))
                continue;

            sprite.LayerSetColor(key, blobbernaut.Color);
        }
    }

    protected override void OnAppearanceChange(EntityUid uid, BlobbernautComponent component, ref AppearanceChangeEvent args)
    {
        UpdateAppearance(uid, component, args.Component, args.Sprite);
    }

    private void OnBlobTileHandleState(EntityUid uid, BlobbernautComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(uid, component);
    }
}
