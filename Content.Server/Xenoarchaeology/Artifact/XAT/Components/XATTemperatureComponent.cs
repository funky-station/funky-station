// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used for an artifact that is activated by having a certain temperature near it.
/// </summary>
[RegisterComponent, Access(typeof(XATTemperatureSystem))]
public sealed partial class XATTemperatureComponent : Component
{
    /// <summary>
    /// Threshold temperature for trigger activation.
    /// </summary>
    [DataField]
    public float TargetTemperature;

    /// <summary>
    /// Marker, if temp needs to be above or below the target.
    /// </summary>
    [DataField]
    public bool TriggerOnHigherTemp = true;
}
