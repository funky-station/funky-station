// SPDX-FileCopyrightText: 2025 mq <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._EE.Carrying;

/// <summary>
/// Stores the carrier of an entity being carried.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CarryingSystem))]
[AutoGenerateComponentState]
public sealed partial class BeingCarriedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Carrier;
}
