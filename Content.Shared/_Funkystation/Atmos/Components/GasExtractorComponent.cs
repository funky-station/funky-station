// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Atmos.Components;

[NetworkedComponent]
[AutoGenerateComponentState]
[RegisterComponent]
public sealed partial class GasExtractorComponent : Component
{
    /// <summary>
    ///     Operational state of the extractor.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public GasExtractorState ExtractorState = GasExtractorState.Disabled;

    /// <summary>
    ///      If the number of moles in the external environment exceeds this number, no gas will be mined.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float MaxExternalAmount = float.PositiveInfinity;

    /// <summary>
    ///      If the pressure (in kPA) of the external environment exceeds this number, no gas will be mined.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField] // Funkystation - Networked for console
    public float MaxExternalPressure = Atmospherics.GasMinerDefaultMaxExternalPressure;

    /// <summary>
    ///     Gas to spawn.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(required: true)]
    public Gas SpawnGas;

    /// <summary>
    ///     Temperature in Kelvin.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float SpawnTemperature = Atmospherics.T20C;

    /// <summary>
    ///     Number of moles created per second when the extractor is working.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField] // Funkystation - Networked for console
    public float SpawnAmount = Atmospherics.MolesCellStandard * 20f;

    /// <summary>
    ///     Moles remaining in the extractor.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public float RemainingMoles = 0f; // Funkystation

    /// <summary>
    ///     Whether the extractor will automatically buy gas.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public bool AutoBuyEnabled = false; // Funkystation
}

[Serializable, NetSerializable]
public enum GasExtractorState : byte
{
    Disabled,
    Idle,
    Working,
}
