// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Content.Client._Funkystation.Effects.ManualPlayback.Components;
using Content.Client._Funkystation.Effects.ManualPlayback.Systems;
using Content.Shared.BloodCult;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Graphics;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using TimedDespawnComponent = Robust.Shared.Spawners.TimedDespawnComponent;

namespace Content.Client._Funkystation.BloodCult.Systems;

public sealed class BloodCultRuneEffectSystem : EntitySystem
{
    private readonly Dictionary<uint, EntityUid> _activeEffects = new();

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private ManualPlaybackEffectSystem? _manualPlaybackSystem;

    private const string TearVeilEffectPrototype = "TearVeilRune_drawing";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RuneDrawingEffectEvent>(OnRuneEffectEvent);
        EntityManager.EntitySysManager.TryGetEntitySystem(out _manualPlaybackSystem);
        EntityManager.EntitySysManager.SystemLoaded += OnSystemLoaded;
        EntityManager.EntitySysManager.SystemUnloaded += OnSystemUnloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        EntityManager.EntitySysManager.SystemLoaded -= OnSystemLoaded;
        EntityManager.EntitySysManager.SystemUnloaded -= OnSystemUnloaded;
    }

    private void OnSystemLoaded(object? sender, SystemChangedArgs args)
    {
        if (args.System is ManualPlaybackEffectSystem manual)
            _manualPlaybackSystem = manual;
    }

    private void OnSystemUnloaded(object? sender, SystemChangedArgs args)
    {
        if (args.System is ManualPlaybackEffectSystem)
            _manualPlaybackSystem = null;
    }

    private void OnRuneEffectEvent(RuneDrawingEffectEvent ev)
    {
        var coordinates = GetCoordinates(ev.Coordinates);

        switch (ev.Action)
        {
            case RuneEffectAction.Start:
                HandleStart(ev, coordinates);
                break;
            case RuneEffectAction.Stop:
                HandleStop(ev);
                break;
        }
    }

    private void HandleStart(RuneDrawingEffectEvent ev, EntityCoordinates coordinates)
    {
        if (string.IsNullOrEmpty(ev.Prototype))
            return;

        if (_activeEffects.TryGetValue(ev.EffectId, out var existing) && !Deleted(existing))
            QueueDel(existing);

        var effect = Spawn(ev.Prototype, coordinates);

        if (!TryComp<SpriteComponent>(effect, out var sprite))
        {
            _activeEffects.Remove(ev.EffectId);
            return;
        }

        var spriteEntity = new Entity<SpriteComponent?>(effect, sprite);
        var manualEffect = EnsureComp<ManualPlaybackStateComponent>(effect);
        manualEffect.Duration = ev.Duration;
        manualEffect.StartTime = _timing.RealTime;
        ResetState(manualEffect);

        manualEffect.LayerKey = EffectLayers.Unshaded;
        manualEffect.RowsPerColumn = 4;
        manualEffect.ColumnsOverride = 0;
        manualEffect.Direction = RsiDirection.South;
        var isTearVeil = string.Equals(ev.Prototype, TearVeilEffectPrototype, StringComparison.Ordinal);

        manualEffect.ManualPlaybackEnabled = !isTearVeil;

        var useManual = manualEffect.ManualPlaybackEnabled
                         && TryGetManualPlaybackSystem(out var manualSystem)
                         && manualSystem.TryEnableManualPlayback(spriteEntity, manualEffect);

        if (!useManual)
        {
            var handledFallback = false;

            if (isTearVeil)
                handledFallback = TryEnableTearVeilFallback(spriteEntity, manualEffect);

            if (!handledFallback)
            {
                manualEffect.ActiveManualPlayback = false;
                manualEffect.Frames = Array.Empty<Texture>();
                manualEffect.FrameDelays = Array.Empty<float>();
                manualEffect.TotalDelay = 0f;

                if (_sprite.TryGetLayer(spriteEntity, manualEffect.LayerKey, out var layer, false))
                {
                    manualEffect.BaseColor = layer.Color;
                    manualEffect.BaseShader = layer.ShaderPrototype;
                    manualEffect.BaseUnshaded = layer.ShaderPrototype == SpriteSystem.UnshadedId;
                    if (!_sprite.LayerMapTryGet(spriteEntity, manualEffect.LayerKey, out var layerIndex, false))
                        layerIndex = -1;
                    manualEffect.LayerIndex = layerIndex;

                    _sprite.LayerSetAutoAnimated(spriteEntity, manualEffect.LayerKey, true);
                    _sprite.LayerSetAnimationTime(spriteEntity, manualEffect.LayerKey, 0f);
                    if (manualEffect.LayerIndex >= 0)
                    {
                        if (manualEffect.BaseUnshaded)
                            sprite.LayerSetShader(manualEffect.LayerIndex, (ShaderInstance?) null, SpriteSystem.UnshadedId.Id);
                        else if (manualEffect.BaseShader is { } shader)
                            sprite.LayerSetShader(manualEffect.LayerIndex, shader.Id);
                        else
                            sprite.LayerSetShader(manualEffect.LayerIndex, (ShaderInstance?) null, null);
                    }
                    _sprite.LayerSetColor(spriteEntity, manualEffect.LayerKey, manualEffect.BaseColor);
                }
            }
        }

        if (TryComp(effect, out TimedDespawnComponent? despawn))
        {
            var manualDuration = manualEffect.ActiveManualPlayback ? Math.Max(manualEffect.TotalDelay, (float) ev.Duration.TotalSeconds) : (float) ev.Duration.TotalSeconds;
            var desiredLifetime = manualDuration + 0.5f;
            if (despawn.Lifetime < desiredLifetime)
                despawn.Lifetime = desiredLifetime;
        }

        _activeEffects[ev.EffectId] = effect;
    }

    private void HandleStop(RuneDrawingEffectEvent ev)
    {
        if (!_activeEffects.TryGetValue(ev.EffectId, out var effectUid))
            return;

        _activeEffects.Remove(ev.EffectId);

        if (Deleted(effectUid))
            return;

        if (!TryComp(effectUid, out ManualPlaybackStateComponent? manualEffect) ||
            !TryComp<SpriteComponent>(effectUid, out var sprite))
        {
            QueueDel(effectUid);
            return;
        }

        var spriteEntity = new Entity<SpriteComponent?>(effectUid, sprite);

        if (manualEffect.ActiveManualPlayback && TryGetManualPlaybackSystem(out var manualSystem))
        {
            manualSystem.ApplyFinalFrame(spriteEntity, manualEffect);
        }

        QueueDel(effectUid);
    }

    private bool TryGetManualPlaybackSystem([NotNullWhen(true)] out ManualPlaybackEffectSystem? system)
    {
        if (_manualPlaybackSystem != null)
        {
            system = _manualPlaybackSystem;
            return true;
        }

        if (EntityManager.EntitySysManager.TryGetEntitySystem<ManualPlaybackEffectSystem>(out var resolved))
        {
            _manualPlaybackSystem = resolved;
            system = resolved;
            return true;
        }

        system = null;
        return false;
    }

    private static void ResetState(ManualPlaybackStateComponent state)
    {
        state.ActiveManualPlayback = false;
        state.Frames = Array.Empty<Texture>();
        state.FrameDelays = Array.Empty<float>();
        state.TotalDelay = 0f;
        state.BaseShader = null;
        state.BaseUnshaded = false;
        state.BaseColor = Color.White;
        state.LayerIndex = -1;
    }

    private bool TryEnableTearVeilFallback(Entity<SpriteComponent?> spriteEntity, ManualPlaybackStateComponent manualEffect)
    {
        if (!_sprite.TryGetLayer(spriteEntity, manualEffect.LayerKey, out var layer, false))
            return false;

        manualEffect.BaseShader = layer.ShaderPrototype;
        manualEffect.BaseUnshaded = layer.ShaderPrototype == SpriteSystem.UnshadedId;
        if (!_sprite.LayerMapTryGet(spriteEntity, manualEffect.LayerKey, out var layerIndex, false))
            layerIndex = -1;
        manualEffect.LayerIndex = layerIndex;

        var state = layer.ActualState;
        if (state == null)
            return false;

        var frames = state.GetFrames(manualEffect.Direction);
        if (frames.Length == 0)
            return false;

        var delays = state.GetDelays();
        var frameDelays = new float[frames.Length];

        for (var i = 0; i < frames.Length; i++)
            frameDelays[i] = delays.Length > i ? delays[i] : 0.1f;

        manualEffect.Frames = frames;
        manualEffect.FrameDelays = frameDelays;

        var totalDelay = 0f;
        foreach (var delay in frameDelays)
            totalDelay += delay;

        if (totalDelay <= 0f)
            totalDelay = frameDelays.Length * 0.1f;

        manualEffect.TotalDelay = totalDelay;
        manualEffect.BaseColor = layer.Color;
        manualEffect.ActiveManualPlayback = true;

        _sprite.LayerSetAutoAnimated(spriteEntity, manualEffect.LayerKey, false);
        _sprite.LayerSetTexture(spriteEntity, manualEffect.LayerKey, frames[0]);
        _sprite.LayerSetColor(spriteEntity, manualEffect.LayerKey, manualEffect.BaseColor);

        if (manualEffect.LayerIndex >= 0)
        {
            if (manualEffect.BaseUnshaded)
                spriteEntity.Comp?.LayerSetShader(manualEffect.LayerIndex, (ShaderInstance?) null, SpriteSystem.UnshadedId.Id);
            else if (manualEffect.BaseShader is { } shader)
                spriteEntity.Comp?.LayerSetShader(manualEffect.LayerIndex, shader.Id);
            else
                spriteEntity.Comp?.LayerSetShader(manualEffect.LayerIndex, (ShaderInstance?) null, null);
        }

        return true;
    }
}
