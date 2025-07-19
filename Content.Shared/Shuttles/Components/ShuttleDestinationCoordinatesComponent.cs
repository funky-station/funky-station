// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Shuttles.Components;
using Robust.Shared.GameStates;

/// <summary>
/// Enables a shuttle to travel to a destination with an item inserted into its console
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShuttleDestinationCoordinatesComponent : Component
{
    /// <summary>
    /// Uid for entity containing the FTLDestination component
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public EntityUid? Destination;
}
