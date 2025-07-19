// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Binary.Components
{
    public sealed record GasHeatPumpData(float LastMolesTransferred);

    [Serializable, NetSerializable]
    public enum GasHeatPumpUiKey
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string PumpLabel { get; }
        public float TransferRate { get; }
        public bool Enabled { get; }

        public GasHeatPumpBoundUserInterfaceState(string pumpLabel, float transferRate, bool enabled)
        {
            PumpLabel = pumpLabel;
            TransferRate = transferRate;
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpToggleStatusMessage : BoundUserInterfaceMessage
    {
        public bool Enabled { get; }

        public GasHeatPumpToggleStatusMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpChangeTransferRateMessage : BoundUserInterfaceMessage
    {
        public float TransferRate { get; }

        public GasHeatPumpChangeTransferRateMessage(float transferRate)
        {
            TransferRate = transferRate;
        }
    }
}
