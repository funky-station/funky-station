// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Light;
using Content.Shared.Light.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Light.EntitySystems;

public sealed class RotatingLightSystem : SharedRotatingLightSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animations = default!;

    private Animation GetAnimation(float speed)
    {
        var third = 120f / speed;
        return new Animation()
        {
            Length = TimeSpan.FromSeconds(360f / speed),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(PointLightComponent),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    Property = nameof(PointLightComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(120), third),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(240), third),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(360), third)
                    }
                }
            }
        };
    }

    private const string AnimKey = "rotating_light";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RotatingLightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RotatingLightComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
        SubscribeLocalEvent<RotatingLightComponent, AnimationCompletedEvent>(OnAnimationComplete);
    }

    private void OnStartup(EntityUid uid, RotatingLightComponent comp, ComponentStartup args)
    {
        var player = EnsureComp<AnimationPlayerComponent>(uid);
        PlayAnimation(uid, comp, player);
    }

    private void OnAfterAutoHandleState(EntityUid uid, RotatingLightComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<AnimationPlayerComponent>(uid, out var player))
            return;

        if (comp.Enabled)
        {
            PlayAnimation(uid, comp, player);
        }
        else
        {
            _animations.Stop(uid, player, AnimKey);
        }
    }

    private void OnAnimationComplete(EntityUid uid, RotatingLightComponent comp, AnimationCompletedEvent args)
    {
        if (!args.Finished)
            return;

        PlayAnimation(uid, comp);
    }

    /// <summary>
    /// Play the light rotation animation.
    /// </summary>
    public void PlayAnimation(EntityUid uid, RotatingLightComponent? comp = null, AnimationPlayerComponent? player = null)
    {
        if (!Resolve(uid, ref comp, ref player) || !comp.Enabled)
            return;

        if (!_animations.HasRunningAnimation(uid, player, AnimKey))
        {
            _animations.Play((uid, player), GetAnimation(comp.Speed), AnimKey);
        }
    }
}
