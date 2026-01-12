// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// Component for the chronic migraines trait.
/// </summary>
[RegisterComponent, Access(typeof(ChronicMigrainesSystem)), NetworkedComponent]
public sealed partial class ChronicMigrainesComponent : Component
{
    /// <summary>
    /// Time range between migraines (min, max).
    /// Default: 8-20 minutes
    /// </summary>
    [DataField]
    public (TimeSpan Min, TimeSpan Max) TimeBetweenMigraines = (TimeSpan.FromMinutes(8), TimeSpan.FromMinutes(20));

    /// <summary>
    /// Duration range for migraines (min, max).
    /// </summary>
    [DataField]
    public (TimeSpan Min, TimeSpan Max) MigraineDuration = (TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));

    /// <summary>
    /// How long the fadeout should take when migraines end (in seconds).
    /// </summary>
    [DataField]
    public float FadeOutDuration { get; private set; } = 0.5f;

    /// <summary>
    /// Time until next migraine.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public TimeSpan NextMigraineTime;
}
