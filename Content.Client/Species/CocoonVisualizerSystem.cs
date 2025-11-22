using Content.Shared.Species.Arachnid;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Maths;

namespace Content.Client.Species;

public sealed class CocoonVisualizerSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CocoonRotationAnimationEvent>(OnCocoonRotationAnimation);
    }

    private void OnCocoonRotationAnimation(CocoonRotationAnimationEvent args)
    {
        var cocoon = GetEntity(args.Cocoon);
        HandleCocoonSpawnAnimation(cocoon);
    }

    /// <summary>
    /// Custom function to handle the instant rotation animation when a cocoon is spawned.
    /// Called from server via networked event.
    /// </summary>
    public void HandleCocoonSpawnAnimation(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        // Play instant 90 degree rotation animation
        PlayInstantRotationAnimation(uid, sprite, Angle.FromDegrees(90));
    }

    private void PlayInstantRotationAnimation(EntityUid uid, SpriteComponent spriteComp, Angle rotation)
    {
        if (spriteComp.Rotation.Equals(rotation))
            return;

        var animationComp = EnsureComp<AnimationPlayerComponent>(uid);
        const string animationKey = "cocoon-instant-rotate";

        // Stop any existing animation
        if (_animation.HasRunningAnimation(animationComp, animationKey))
        {
            _animation.Stop((uid, animationComp), animationKey);
        }

        // Create instant animation with single keyframe at 90 degrees
        var animation = new Animation
        {
            Length = TimeSpan.Zero,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(rotation, 0)
                    }
                }
            }
        };

        _animation.Play((uid, animationComp), animation, animationKey);
    }
}

