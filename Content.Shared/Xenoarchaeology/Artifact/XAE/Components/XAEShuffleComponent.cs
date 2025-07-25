// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// When activated, will shuffle the position of all players
/// within a certain radius.
/// </summary>
[RegisterComponent, Access(typeof(XAEShuffleSystem)), NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XAEShuffleComponent : Component
{
    /// <summary>
    /// Radius, within which mobs would be switched.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Radius = 7.5f;
}
