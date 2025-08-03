// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used a XAT that activates when an entity fulfilling the given whitelist is nearby the artifact.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(XATCompNearbyComponent)), AutoGenerateComponentState]
public sealed partial class XATCompNearbyComponent : Component
{
    /// <summary>
    /// Component name that is required to activate trigger.
    /// Is spelled without 'Component' suffix.
    /// </summary>
    [DataField(customTypeSerializer: typeof(ComponentNameSerializer)), AutoNetworkedField]
    public string RequireComponentWithName = "Item";

    /// <summary>
    /// Radius, in which trigger going to search for entity with component.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Radius = 5;

    /// <summary>
    /// Required entities count.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Count = 1;
}
