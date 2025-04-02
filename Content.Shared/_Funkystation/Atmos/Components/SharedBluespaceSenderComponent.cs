using Content.Shared.Materials;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Components
{
    /// <summary>
    /// Key representing which <see cref="PlayerBoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum BluespaceSenderUiKey
    {
        Key,
    }

    /// <summary>
    /// Represents a <see cref="BluespaceSenderComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class BluespaceSenderBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string BluespaceSenderLabel { get; }
        public GasMixture BluespaceGasMixture { get; }
        public List<bool> BluespaceSenderEnabledList { get; }
        public List<bool> BluespaceSenderRetrieveList { get; }

        public BluespaceSenderBoundUserInterfaceState(string bluespaceSenderLabel, GasMixture bluespaceGasMixture, List<bool> bluespaceSenderEnabledList, List<bool> bluespaceSenderRetrieveList)
        {
            BluespaceSenderLabel = bluespaceSenderLabel;
            BluespaceGasMixture = bluespaceGasMixture;
            BluespaceSenderEnabledList = bluespaceSenderEnabledList;
            BluespaceSenderRetrieveList = bluespaceSenderRetrieveList;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceSenderChangeRetrieveMessage : BoundUserInterfaceMessage
    {
        public int Index { get; }
        public BluespaceSenderChangeRetrieveMessage(int index)
        {
            Index = index;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceSenderChangeEnabledGasesMessage : BoundUserInterfaceMessage
    {
        public int Index { get; }
        public BluespaceSenderChangeEnabledGasesMessage(int index)
        {
            Index = index;
        }
    }
}