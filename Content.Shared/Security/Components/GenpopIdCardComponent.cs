// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JoulesBerg <104539820+JoulesBerg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Security.Components;

/// <summary>
/// This is used for storing information about a Genpop ID in order to correctly display it on examine.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class GenpopIdCardComponent : Component
{
    /// <summary>
    /// The crime committed, as a string.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Crime = string.Empty;

    /// <summary>
    /// The length of the sentence
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan SentenceDuration;
}
