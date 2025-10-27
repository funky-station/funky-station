// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.DeviceNetwork.Components
{
    [RegisterComponent]
    public sealed partial class StationLimitedNetworkComponent : Component
    {
        /// <summary>
        /// The station id the device is limited to.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid? StationId;

        /// <summary>
        /// Whether the entity is allowed to receive packets from entities that are not tied to any station
        /// </summary>
        [DataField("allowNonStationPackets")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool AllowNonStationPackets = false;
    }
}
