// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System;
using Content.Client._Funkystation.Effects.ManualPlayback.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Graphics;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Timing;

namespace Content.Client._Funkystation.Effects.ManualPlayback.Systems;

/// <summary>
/// Generic system that supports manually driving sprite animations using frame metadata.
/// </summary>
public sealed class ManualPlaybackEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        var enumerator = EntityQueryEnumerator<ManualPlaybackStateComponent, SpriteComponent>();

        while (enumerator.MoveNext(out var uid, out var effect, out var sprite))
        {
            if (!effect.ActiveManualPlayback || effect.Frames.Length == 0)
                continue;

            var spriteEntity = new Entity<SpriteComponent?>(uid, sprite);

            var elapsed = (float) (now - effect.StartTime).TotalSeconds;
            if (elapsed < 0f)
                elapsed = 0f;

            var total = effect.TotalDelay > 0f ? effect.TotalDelay : 0f;
            if (total <= 0f)
            {
                total = 0f;
                foreach (var delay in effect.FrameDelays)
                    total += delay;
                if (total <= 0f)
                    total = effect.Frames.Length * 0.1f;
                effect.TotalDelay = total;
            }

            var speedScale = 1f;
            if (effect.Duration.TotalSeconds > 0.1f && total > 0f)
                speedScale = total / (float) effect.Duration.TotalSeconds;

            var targetFrame = effect.Frames.Length - 1;
            var accumulated = 0f;

            for (var i = 0; i < effect.FrameDelays.Length; i++)
            {
                accumulated += effect.FrameDelays[i] / speedScale;
                if (elapsed < accumulated)
                {
                    targetFrame = i;
                    break;
                }
            }

            if (elapsed >= total / speedScale)
                targetFrame = effect.Frames.Length - 1;

            if ((uint) targetFrame >= effect.Frames.Length)
                targetFrame = effect.Frames.Length - 1;

            _sprite.LayerSetTexture(spriteEntity, effect.LayerKey, effect.Frames[targetFrame]);

            if (effect.LayerIndex >= 0)
            {
                if (effect.BaseUnshaded)
                    sprite.LayerSetShader(effect.LayerIndex, (ShaderInstance?) null, SpriteSystem.UnshadedId.Id);
                else if (effect.BaseShader is { } shader)
                    sprite.LayerSetShader(effect.LayerIndex, shader.Id);
                else
                    sprite.LayerSetShader(effect.LayerIndex, (ShaderInstance?) null, null);
            }

            _sprite.LayerSetColor(spriteEntity, effect.LayerKey, effect.BaseColor);
            _sprite.LayerSetVisible(spriteEntity, effect.LayerKey, true);
        }
    }

    public bool TryEnableManualPlayback(Entity<SpriteComponent?> spriteEntity, ManualPlaybackStateComponent effect)
    {
        effect.ActiveManualPlayback = false;

        if (!effect.ManualPlaybackEnabled)
            return false;

        if (!_sprite.TryGetLayer(spriteEntity, effect.LayerKey, out var layer, false))
            return false;

        effect.BaseShader = layer.ShaderPrototype;
        effect.BaseUnshaded = layer.ShaderPrototype == SpriteSystem.UnshadedId;
        if (!_sprite.LayerMapTryGet(spriteEntity, effect.LayerKey, out var layerIndex, false))
            layerIndex = -1;
        effect.LayerIndex = layerIndex;

        var state = layer.ActualState;
        if (state == null)
            return false;

        var frames = state.GetFrames(effect.Direction);
        var delays = state.GetDelays();
        var frameCount = frames.Length;

        if (frameCount == 0)
            return false;

        var rows = Math.Max(1, effect.RowsPerColumn);
        var columns = effect.ColumnsOverride > 0 ? effect.ColumnsOverride : Math.Max(1, (frameCount + rows - 1) / rows);

        var orderedFrames = new Texture[frameCount];
        var orderedDelays = new float[frameCount];

        var index = 0;
        for (var col = 0; col < columns; col++)
        {
            for (var row = 0; row < rows; row++)
            {
                var originalIndex = row * columns + col;
                if (originalIndex >= frameCount)
                    continue;

                orderedFrames[index] = frames[originalIndex];
                orderedDelays[index] = delays.Length > originalIndex ? delays[originalIndex] : 0.1f;
                index++;
            }
        }

        if (index < frameCount)
        {
            Array.Resize(ref orderedFrames, index);
            Array.Resize(ref orderedDelays, index);
        }

        effect.Frames = orderedFrames;
        effect.FrameDelays = orderedDelays;
        effect.TotalDelay = 0f;
        foreach (var delay in effect.FrameDelays)
            effect.TotalDelay += delay;

        if (effect.TotalDelay <= 0f)
            effect.TotalDelay = effect.FrameDelays.Length * 0.1f;

        var baseColor = layer.Color;

        _sprite.LayerSetAutoAnimated(spriteEntity, effect.LayerKey, false);
        if (effect.Frames.Length > 0)
        {
            _sprite.LayerSetTexture(spriteEntity, effect.LayerKey, effect.Frames[0]);
            _sprite.LayerSetColor(spriteEntity, effect.LayerKey, baseColor);
        }

        effect.BaseColor = baseColor;
        effect.ActiveManualPlayback = true;
        return true;
    }

    public void DisableManualPlayback(Entity<SpriteComponent?> spriteEntity, ManualPlaybackStateComponent effect)
    {
        effect.ActiveManualPlayback = false;
        effect.Frames = Array.Empty<Texture>();
        effect.FrameDelays = Array.Empty<float>();
        effect.TotalDelay = 0f;
        _sprite.LayerSetAutoAnimated(spriteEntity, effect.LayerKey, true);
    }

    public void ApplyFinalFrame(Entity<SpriteComponent?> spriteEntity, ManualPlaybackStateComponent effect)
    {
        if (!effect.ActiveManualPlayback || effect.Frames.Length == 0)
            return;

        var spriteComp = spriteEntity.Comp;
        if (spriteComp == null)
            return;

        _sprite.LayerSetTexture(spriteEntity, effect.LayerKey, effect.Frames[^1]);

        if (effect.LayerIndex >= 0)
        {
            if (effect.BaseUnshaded)
                spriteComp.LayerSetShader(effect.LayerIndex, (ShaderInstance?) null, SpriteSystem.UnshadedId.Id);
            else if (effect.BaseShader is { } shader)
                spriteComp.LayerSetShader(effect.LayerIndex, shader.Id);
            else
                spriteComp.LayerSetShader(effect.LayerIndex, (ShaderInstance?) null, null);
        }

        _sprite.LayerSetColor(spriteEntity, effect.LayerKey, effect.BaseColor);
    }
}

