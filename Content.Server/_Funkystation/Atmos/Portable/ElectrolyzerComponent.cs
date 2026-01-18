// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

// Assmos - /tg/ gases
using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Atmos.Portable;

[RegisterComponent]
public sealed partial class ElectrolyzerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float CurrentFuel { get; set; } = 0f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float PlasmaFuelConversion { get; set; } = 25000f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float UraniumFuelConversion { get; set; } = 150000f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool IsPowered { get; set; } = false;

    [DataField("onSound")]
    public SoundSpecifier? OnSound;
}
