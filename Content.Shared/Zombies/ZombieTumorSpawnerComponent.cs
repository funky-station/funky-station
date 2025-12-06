// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Zombies;

/// <summary>
/// Component that periodically attempts to spawn a zombie tumor organ in the entity's body.
/// Removes itself once the tumor is successfully added.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
[Access(typeof(SharedZombieTumorOrganSystem))]
public sealed partial class ZombieTumorSpawnerComponent : Component
{
    /// <summary>
    /// Next time to attempt spawning the tumor organ.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAttempt = TimeSpan.Zero;
}

