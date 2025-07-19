// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.BrittleBones;

/// <summary>
/// Component that makes an entity enter critical condition sooner due to brittle bones
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BrittleBonesComponent : Component
{
    /// <summary>
    /// How much to modify the critical health threshold by.
    /// Negative values mean entering crit sooner.
    /// </summary>
    [DataField("criticalThresholdModifier"), AutoNetworkedField]
    public float CriticalThresholdModifier = -50f;
}
