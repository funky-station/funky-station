// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos;
using Content.Shared.Guidebook;

namespace Content.Server._Funkystation.Atmos.Portable
{
    [RegisterComponent]
    public sealed partial class PipeScrubberComponent : Component
    {
        /// <summary>
        /// The air stored inside this pipe scrubber.
        /// </summary>
        [DataField("gasMixture"), ViewVariables(VVAccess.ReadWrite)]
        public GasMixture Air { get; private set; } = new();

        [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
        public string PortName { get; set; } = "port";

        /// <summary>
        /// Maximum internal pressure before it stops accepting more gas.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MaxPressure = 9000f;

        /// <summary>
        /// The rate (L/s) at which gas is transferred from the pipe network.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float TransferRate = 50f;

        /// <summary>
        /// Whether the scrubber is currently active (sucking gas from the pipe network).
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Enabled { get; set; } = false;

        #region GuidebookData

        [GuidebookData]
        public float Volume => Air.Volume;

        #endregion
    }
}
