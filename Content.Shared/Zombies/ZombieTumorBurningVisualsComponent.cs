// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Zombies;

/// <summary>
/// Component that tracks visual states for zombie tumors (normal, burning, dead).
/// Used by client visualizer to change sprite states.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZombieTumorBurningVisualsComponent : Component
{
    /// <summary>
    /// Normal sprite state (zombietumor or zombierobotumor)
    /// </summary>
    [DataField, AutoNetworkedField]
    public string NormalState = "zombietumor";

    /// <summary>
    /// Burning sprite state (organ_burning)
    /// </summary>
    [DataField, AutoNetworkedField]
    public string BurningState = "organ_burning";

    /// <summary>
    /// Dead sprite state (dead)
    /// </summary>
    [DataField, AutoNetworkedField]
    public string DeadState = "dead";
}

/// <summary>
/// Appearance data keys for zombie tumor visual states.
/// </summary>
[Serializable, NetSerializable]
public enum ZombieTumorBurningVisuals : byte
{
    Dead
}
