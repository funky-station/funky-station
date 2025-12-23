// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Component for migraines.
/// Note: Movement slowdown is handled by MovementSpeedModifierSystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MigraineComponent : Component
{
    /// <summary>
    /// The target magnitude of vision impairment during the migraine (0-6).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float BlurryMagnitude = 4f;

    /// <summary>
    /// Current interpolated blur value (smoothly transitions to target).
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float CurrentBlur = 0f;

    /// <summary>
    /// Time accumulated for pulsing effect.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float PulseAccumulator = 0f;

    /// <summary>
    /// How fast the blur ramps up (units per second). Lower = slower fade-in
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float RampUpSpeed = 0.5f;

    /// <summary>
    /// Whether this migraine should apply the movement slowdown effect when initialized.
    /// Prodromes can set this to false to only show the visual overlay without slowing the player.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ApplySlowdown = true;

    /// <summary>
    /// Slowdown multiplier used if <see cref="ApplySlowdown"/> is true
    /// Default 0.7 mirrors previous hardcoded behavior.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SlowdownFactor = 0.7f;

    /// <summary>
    /// How fast the blur ramps down (units per second). Lower = slower fade-out
    /// If not specified, clients will default to the RampUpSpeed value.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("rampDownSpeed"), AutoNetworkedField]
    public float RampDownSpeed = 2f;

    /// <summary>
    /// Frequency of the pulsing effect (cycles per second).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float PulseFrequency = 0.4f;

    /// <summary>
    /// Amplitude of the pulsing effect (how much it varies from base)
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    // Much lower amplitude for a subtler pulse
    public float PulseAmplitude = 0.8f;

    /// <summary>
    /// Probably used for reduce motion
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("useSoftShader"), AutoNetworkedField]
    public bool UseSoftShader;

    /// <summary>
    /// Multiplier applied to shader strength  when UseSoftShader is true
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("softness"), AutoNetworkedField]
    public float Softness = 0.45f;

    /// <summary>
    /// This component instance was created to fade out visuals after the status effect ended.
    /// If true, the system will automatically remove it once CurrentBlur and pulse are negligible.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("isFading"), AutoNetworkedField]
    public bool IsFading;

    /// <summary>
    /// Duration remaining for this migraine episode. When this reaches 0, the migraine will start fading out.
    /// Set to -1 for infinite.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float Duration = -1f;

    /// <summary>
    /// How long the fadeout should take when the migraine ends (in seconds).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float FadeOutDuration = 0.5f;
}
