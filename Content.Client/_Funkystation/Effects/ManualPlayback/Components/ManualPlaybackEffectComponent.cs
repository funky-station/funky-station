// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.Effects.ManualPlayback.Components;

/// <summary>
/// Client-side state for manually controlling playback of a sprite layer.
/// </summary>
[RegisterComponent]
public sealed partial class ManualPlaybackStateComponent : Component
{
    public TimeSpan Duration;
    public TimeSpan StartTime;

    public EffectLayers LayerKey = EffectLayers.Unshaded;

    public int RowsPerColumn = 4;

    public int ColumnsOverride = 0;

    public RsiDirection Direction = RsiDirection.South;

    public bool ManualPlaybackEnabled = true;

    public bool ActiveManualPlayback;

    public Texture[] Frames = Array.Empty<Texture>();
    public float[] FrameDelays = Array.Empty<float>();
    public float TotalDelay;
    public Color BaseColor = Color.White;

    public ProtoId<ShaderPrototype>? BaseShader;

    public bool BaseUnshaded;
    public int LayerIndex = -1;
}

