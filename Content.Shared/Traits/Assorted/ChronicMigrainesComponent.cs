// SPDX-FileCopyrightText: 2026 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Component for the chronic migraines trait.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChronicMigrainesComponent : Component
{
    /// <summary>
    /// Time range between migraines (min, max).
    /// Default: 8-20 minutes
    /// </summary>
    [DataField, AutoNetworkedField]
    public (TimeSpan Min, TimeSpan Max) TimeBetweenMigraines = (TimeSpan.FromMinutes(8), TimeSpan.FromMinutes(20));

    /// <summary>
    /// Duration range for migraines (min, max).
    /// </summary>
    [DataField, AutoNetworkedField]
    public (TimeSpan Min, TimeSpan Max) MigraineDuration = (TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));

    /// <summary>
    /// How long the fadeout should take when migraines end (in seconds).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FadeOutDuration = 0.5f;

    /// <summary>
    /// Time until next migraine.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextMigraineTime;
}

