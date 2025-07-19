// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used for a xenoarch trigger that activates when something dies nearby.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(XATDeathSystem)), AutoGenerateComponentState]
public sealed partial class XATDeathComponent : Component
{
    /// <summary>
    /// Range within which artifact going to listen to death event.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 15;
}
