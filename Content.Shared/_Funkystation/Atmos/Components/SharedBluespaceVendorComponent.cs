// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;

namespace Content.Shared._Funkystation.Atmos.Components
{
    /// <summary>
    /// Key representing which <see cref="PlayerBoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum BluespaceVendorUiKey
    {
        Key,
    }
    [NetSerializable, Serializable]
    public enum BluespaceVendorVisualLayers
    {
        Tank,
        Pumping
    }
    [NetSerializable, Serializable]
    public enum BluespaceVendorVisuals
    {
        TankInserted,
        isPumping
    }

    /// <summary>
    /// Represents a <see cref="BluespaceVendorComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class BluespaceVendorBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string BluespaceVendorLabel { get; }
        public string? TankLabel { get; }
        public List<bool>  BluespaceVendorRetrieveList { get; }
        public GasMixture BluespaceGasMixture { get; }
        public GasMixture TankGasMixture { get; }
        public float ReleasePressure { get; }
        public bool BluespaceSenderConnected { get; }

        public BluespaceVendorBoundUserInterfaceState(string bluespaceVendorLabel, string? tankLabel, List<bool> bluespaceVendorRetrieveList, GasMixture bluespaceGasMixture, GasMixture tankGasMixture, float releasePressure, bool bluespaceSenderConnected)
        {
            BluespaceVendorLabel = bluespaceVendorLabel;
            TankLabel = tankLabel;
            BluespaceVendorRetrieveList = bluespaceVendorRetrieveList;
            BluespaceGasMixture = bluespaceGasMixture;
            TankGasMixture = tankGasMixture;
            ReleasePressure = releasePressure;
            BluespaceSenderConnected = bluespaceSenderConnected;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceVendorHoldingTankEjectMessage : BoundUserInterfaceMessage
    {
        public BluespaceVendorHoldingTankEjectMessage()
        {}
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceSenderConnectedMessage : BoundUserInterfaceMessage
    {
        public BluespaceSenderConnectedMessage()
        {}
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceVendorHoldingTankEmptyMessage : BoundUserInterfaceMessage
    {
        public BluespaceVendorHoldingTankEmptyMessage()
        {}
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceVendorFillTankMessage : BoundUserInterfaceMessage
    {
        public int Index { get; }
        public BluespaceVendorFillTankMessage(int index)
        {
            Index = index;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceVendorChangeRetrieveMessage : BoundUserInterfaceMessage
    {
        public int Index { get; }
        public BluespaceVendorChangeRetrieveMessage(int index)
        {
            Index = index;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceVendorChangeReleasePressureMessage : BoundUserInterfaceMessage
    {
        public float Pressure { get; }
        public BluespaceVendorChangeReleasePressureMessage(float pressure)
        {
            Pressure = pressure;
        }
    }
}
