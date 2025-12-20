// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// This is used for the chronic migraines trait.
/// </summary>
[RegisterComponent, Access(typeof(ChronicMigrainesSystem))]
public sealed partial class ChronicMigrainesComponent : Component
{
    /// <summary>
    /// The random time between migraine episodes, (min, max) in seconds.
    /// </summary>
    [DataField("timeBetweenIncidents", required: true)]
    public Vector2 TimeBetweenMigraines { get; private set; } = new(0f, 0f);

    /// <summary>
    /// The duration of migraine episodes, (min, max) in seconds.
    /// </summary>
    [DataField("durationOfIncident", required: true)]
    // Default duration: 8 to 12 seconds
    public Vector2 MigraineDuration { get; private set; } = new(8f, 12f);

    /// <summary>
    /// How long the fadeout should take when migraines end (in seconds).
    /// </summary>
    [DataField("fadeOutDuration")]
    public float FadeOutDuration { get; private set; } = 0.5f;

    public float NextMigraineTime;
}
