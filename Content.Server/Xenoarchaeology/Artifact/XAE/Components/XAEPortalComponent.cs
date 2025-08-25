// SPDX-FileCopyrightText: 2025 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
///     When activated artifact will spawn a pair of portals. First - right in artifact, Second - at random point of station.
/// </summary>
[RegisterComponent, Access(typeof(XAEPortalSystem))]
public sealed partial class XAEPortalComponent : Component
{
    /// <summary>
    /// Entity that should be spawned as portal.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId PortalProto = "PortalArtifact";
}
