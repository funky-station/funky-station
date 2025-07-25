// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.Abilities;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._DV.Abilities;

public sealed partial class HideUnderTableAbilitySystem : SharedCrawlUnderObjectsSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid,
        CrawlUnderObjectsComponent component,
        AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _appearance.TryGetData(uid, SneakMode.Enabled, out bool enabled);
        if (enabled)
        {
            if (component.OriginalDrawDepth != null)
                return;

            component.OriginalDrawDepth = sprite.DrawDepth;
            sprite.DrawDepth = (int) DrawDepth.SmallMobs;
        }
        else
        {
            if (component.OriginalDrawDepth == null)
                return;

            sprite.DrawDepth = (int) component.OriginalDrawDepth;
            component.OriginalDrawDepth = null;
        }
    }
}
