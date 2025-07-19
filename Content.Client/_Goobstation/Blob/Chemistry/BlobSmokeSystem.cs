// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared._Goobstation.Blob.Chemistry;
using Content.Shared.Chemistry.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Chemistry;

public sealed class BlobSmokeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobSmokeColorComponent, AfterAutoHandleStateEvent>(OnBlobTileHandleState);
    }

    private void OnBlobTileHandleState(EntityUid uid, BlobSmokeColorComponent component, ref AfterAutoHandleStateEvent state)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            sprite.LayerSetColor(i, component.Color);
        }
    }
}
