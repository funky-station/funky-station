// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Atmos.Piping.Binary.Components;

[RegisterComponent]
public sealed partial class TemperatureGateComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("threshold")]
    public float Threshold { get; set; } = 273.15f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("inverted")]
    public bool Inverted { get; set; } = false;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("enabled")]
    public bool Enabled { get; set; } = false;

    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    [ViewVariables]
    public float? LastInputTemperature { get; set; }
}
