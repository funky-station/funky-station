// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Polymorph.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Polymorph.Components;

/// <summary>
/// Added to a player when they use a chameleon projector.
/// Handles making them invisible and revealing when damaged enough or switching hands.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedChameleonProjectorSystem))]
[AutoGenerateComponentState]
public sealed partial class ChameleonDisguisedComponent : Component
{
    /// <summary>
    /// The disguise entity parented to the player.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Disguise;

    /// <summary>
    /// For client, whether the user's sprite was previously visible or not.
    /// </summary>
    [DataField]
    public bool WasVisible;
}
