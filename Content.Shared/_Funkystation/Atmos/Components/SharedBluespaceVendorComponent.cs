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

    /// <summary>
    /// Represents a <see cref="BluespaceVendorComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class BluespaceVendorBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string BluespaceVendorLabel { get; }
        public string? TankLabel { get; }
        public float TankPressure { get; }
        public float[] ReleasePressure { get; }
        public float ReleasePressureMin { get; }
        public float ReleasePressureMax { get; }
        public GasMixture BluespaceGasMixture { get; }
        public GasMixture TankGasMixture { get; }
        public bool BluespaceSenderConnected { get; }
        public List<bool> BluespaceSenderEnabledList { get; }

        public BluespaceVendorBoundUserInterfaceState(string bluespaceVendorLabel, string? tankLabel, float tankPressure, float[] releasePressure, float releaseValveMin, float releaseValveMax, GasMixture bluespaceGasMixture, GasMixture tankGasMixture, bool bluespaceSenderConnected, List<bool> bluespaceSenderEnabledList)
        {
            BluespaceVendorLabel = bluespaceVendorLabel;
            TankLabel = tankLabel;
            TankPressure = tankPressure;
            ReleasePressureMin = releaseValveMin;
            ReleasePressureMax = releaseValveMax;
            ReleasePressure = releasePressure;
            BluespaceGasMixture = bluespaceGasMixture;
            TankGasMixture = tankGasMixture;
            BluespaceSenderConnected = bluespaceSenderConnected;
            BluespaceSenderEnabledList = bluespaceSenderEnabledList;
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
    public sealed class BluespaceVendorChangeReleasePressureMessage : BoundUserInterfaceMessage
    {
        public float Pressure { get; }
        public int Index { get; }

        public BluespaceVendorChangeReleasePressureMessage(float pressure, int index)
        {
            Index = index;
            Pressure = pressure;
        }
    }
}
