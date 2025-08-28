// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Worldgen.Components.Carvers;
using Content.Server.Worldgen.Systems.Debris;

namespace Content.Server.Worldgen.Systems.Carvers;

/// <summary>
///     This handles carving out holes in world generation according to a noise channel.
/// </summary>
public sealed class NoiseRangeCarverSystem : EntitySystem
{
    [Dependency] private readonly NoiseIndexSystem _index = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        SubscribeLocalEvent<NoiseRangeCarverComponent, PrePlaceDebrisFeatureEvent>(OnPrePlaceDebris);
    }

    private void OnPrePlaceDebris(EntityUid uid, NoiseRangeCarverComponent component,
        ref PrePlaceDebrisFeatureEvent args)
    {
        var coords = WorldGen.WorldToChunkCoords(_transform.ToMapCoordinates(args.Coords).Position);
        var val = _index.Evaluate(uid, component.NoiseChannel, coords);

        foreach (var (low, high) in component.Ranges)
        {
            if (low > val || high < val)
                continue;

            args.Handled = true;
            return;
        }
    }
}

