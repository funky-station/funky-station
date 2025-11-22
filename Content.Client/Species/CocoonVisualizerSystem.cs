using Content.Shared.Rotation;
using Content.Shared.Species.Arachnid;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Maths;

namespace Content.Client.Species;

public sealed class CocoonVisualizerSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CocoonRotationAnimationEvent>(OnCocoonRotationAnimation);
    }

    private void OnCocoonRotationAnimation(CocoonRotationAnimationEvent args)
    {
        var cocoon = GetEntity(args.Cocoon);
        HandleCocoonSpawnAnimation(cocoon, args.VictimWasStanding);
    }

    /// <summary>
    /// Custom function to handle the rotation animation when a cocoon is spawned.
    /// Called from server via networked event.
    /// </summary>
    public void HandleCocoonSpawnAnimation(EntityUid uid, bool victimWasStanding)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (victimWasStanding)
        {
            // If victim was standing, use appearance system for smooth animation
            _appearance.SetData(uid, RotationVisuals.RotationState, RotationState.Horizontal);
        }
        else
        {
            // If victim was already down, play instant 90 degree rotation animation
            PlayInstantRotationAnimation(uid, sprite, Angle.FromDegrees(90));
        }
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

