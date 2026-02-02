// SPDX-FileCopyrightText: 2025 mq <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._EE.Carrying;

[RegisterComponent, NetworkedComponent, Access(typeof(CarryingSlowdownSystem))]
[AutoGenerateComponentState]
public sealed partial class CarryingSlowdownComponent : Component
{
    /// <summary>
    /// Modifier for both walk and sprint speed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Modifier = 1.0f;
}
