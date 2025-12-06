// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Body.Organ;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Zombies;

/// <summary>
/// Component for the zombie tumor organ that spreads infection via range-based auras.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedZombieTumorOrganSystem))]
public sealed partial class ZombieTumorOrganComponent : Component
{
    /// <summary>
    /// Range at which the organ can infect nearby entities.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InfectionRange = 5f;

    /// <summary>
    /// Base infection chance per organ (stacks with multiple organs).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BaseInfectionChance = 0.05f;

    /// <summary>
    /// How often to update the infection check (in seconds).
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Next time to update the infection check.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
}
