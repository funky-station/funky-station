// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Component for migraine visual effects.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MigraineComponent : Component
{
    /// <summary>
    /// Target magnitude of vision impairment (0-6 scale).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float BlurryMagnitude = 4f;

    /// <summary>
    /// How fast the blur ramps up (units per second).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float RampUpSpeed = 0.5f;

    /// <summary>
    /// How fast the blur ramps down (units per second).
    /// Defaults to RampUpSpeed if not specified.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("rampDownSpeed"), AutoNetworkedField]
    public float RampDownSpeed = 2f;

    /// <summary>
    /// Pulsing effect frequency (cycles per second).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float PulseFrequency = 0.4f;

    /// <summary>
    /// Pulsing effect amplitude (variation from base blur).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float PulseAmplitude = 0.8f;

    /// <summary>
    /// Whether to use soft shader effects (accessibility option).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("useSoftShader"), AutoNetworkedField]
    public bool UseSoftShader;

    /// <summary>
    /// Shader strength multiplier when UseSoftShader is enabled.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("softness"), AutoNetworkedField]
    public float Softness = 0.45f;

    // --- Movement Configuration ---

    /// <summary>
    /// Whether to apply movement slowdown when migraine starts.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ApplySlowdown = true;

    /// <summary>
    /// Movement speed multiplier when ApplySlowdown is true.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SlowdownFactor = 0.7f;

    /// <summary>
    /// Duration remaining for this migraine episode (in seconds).
    /// When this reaches 0, migraine starts fading. Set to -1 for infinite.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float Duration = -1f;

    /// <summary>
    /// How long the fadeout takes when migraine ends (in seconds).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float FadeOutDuration = 0.5f;

    /// <summary>
    /// Current interpolated blur value (smoothly transitions to target).
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float CurrentBlur = 0f;

    /// <summary>
    /// Time accumulator for pulsing effects.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float PulseAccumulator = 0f;

    /// <summary>
    /// Whether this component is fading out and should be removed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("isFading"), AutoNetworkedField]
    public bool IsFading;
}
