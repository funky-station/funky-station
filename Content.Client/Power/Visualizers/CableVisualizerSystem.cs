// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 ashley <vectorassembly@icloud.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 vectorassembly <vctrssmly@icloud.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.SubFloor;
using Content.Shared.Wires;
using Robust.Client.GameObjects;

namespace Content.Client.Power.Visualizers;

public sealed class CableVisualizerSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CableVisualizerComponent, AppearanceChangeEvent>(OnAppearanceChange, after: new[] { typeof(SubFloorHideSystem) });
    }

    private void OnAppearanceChange(EntityUid uid, CableVisualizerComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.Sprite.Visible)
        {
            // This entity is probably below a floor and is not even visible to the user -> don't bother updating sprite data.
            // Note that if the subfloor visuals change, then another AppearanceChangeEvent will get triggered.
            return;
        }

        if (!_appearanceSystem.TryGetData<WireVisDirFlags>(uid, WireVisVisuals.ConnectedMask, out var mask, args.Component))
            mask = WireVisDirFlags.None;

        args.Sprite.LayerSetState(0, $"{component.StatePrefix}{(int) mask}");
        if (component.ExtraLayerPrefix != null)
            args.Sprite.LayerSetState(1, $"{component.ExtraLayerPrefix}{(int) mask}");
    }
}
