// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Server._Funkystation.Atmos.EntitySystems;
using System.Linq;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class BluespaceSenderComponent : Component
    {
        private const int GasTypeCount = Atmospherics.TotalNumberOfGases;

        /// <summary>
        ///     The port name for connecting to a Bluespace vendor.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("port")]
        public string PortName { get; set; } = "port";

        /// <summary>
        ///     Inlet pipe for sender
        /// </summary>
        [DataField("inlet")]
        public string InletName = "pipe";

        /// <summary>
        /// The gas mixture stored in Bluespace.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceGasMixture")]
        public GasMixture BluespaceGasMixture { get; set; } = new();

        /// <summary>
        /// The gas mixture in the input/output pipe.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("pipeGasMixture")]
        public GasMixture PipeGasMixture { get; set; } = new();

        /// <summary>
        ///     List of bools for retrieving gases
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderRetrieveList")]
        public List<bool> BluespaceSenderRetrieveList { get; set; } = Enumerable.Repeat(false, GasTypeCount).ToList();

        /// <summary>
        ///     Bool to determine if sender is powered on
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderToggle")]
        public bool PowerToggle { get; set; } = true;

        /// <summary>
        ///     Bool to determine if sender is in retrieve mode
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderInRetrievingMode")]
        public bool InRetrieveMode { get; set; } = false;
    }
}