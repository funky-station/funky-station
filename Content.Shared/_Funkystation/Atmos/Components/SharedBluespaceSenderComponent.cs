// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

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
        public List<bool> BluespaceSenderRetrieveList { get; }
        public bool PowerToggle { get; }
        public bool InRetrieveMode { get; }

        public BluespaceSenderBoundUserInterfaceState(string bluespaceSenderLabel, GasMixture bluespaceGasMixture, List<bool> bluespaceSenderRetrieveList, bool powerToggle, bool inRetrieveMode)
        {
            BluespaceSenderLabel = bluespaceSenderLabel;
            BluespaceGasMixture = bluespaceGasMixture;
            BluespaceSenderRetrieveList = bluespaceSenderRetrieveList;
            PowerToggle = powerToggle;
            InRetrieveMode = inRetrieveMode;
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
    public sealed class BluespaceSenderToggleMessage : BoundUserInterfaceMessage
    {
        public BluespaceSenderToggleMessage()
        {
        }
    }

    [Serializable, NetSerializable]
    public sealed class BluespaceSenderToggleRetrieveModeMessage : BoundUserInterfaceMessage
    {
        public BluespaceSenderToggleRetrieveModeMessage()
        {
        }
    }
}