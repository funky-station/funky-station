// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used for an artifact that activates when above or below a certain pressure.
/// </summary>
[RegisterComponent, Access(typeof(XATPressureSystem))]
public sealed partial class XATPressureComponent : Component
{
    /// <summary>
    /// The lower-end pressure threshold. Is not considered when null.
    /// </summary>
    [DataField]
    public float? MinPressureThreshold;

    /// <summary>
    /// The higher-end pressure threshold. Is not considered when null.
    /// </summary>
    [DataField]
    public float? MaxPressureThreshold;
}
