// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Clock;

/// <summary>
/// This is used for globally managing the time on-station
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause, Access(typeof(SharedClockSystem))]
public sealed partial class GlobalTimeManagerComponent : Component
{
    /// <summary>
    /// A fixed random offset, used to fuzz the time between shifts.
    /// </summary>
    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan TimeOffset;
}
