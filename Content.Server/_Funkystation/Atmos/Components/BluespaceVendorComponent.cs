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
    public sealed partial class BluespaceVendorComponent : Component
    {
        private const int GasTypeCount = Atmospherics.TotalNumberOfGases;

        /// <summary>
        /// The port name for connecting to a Bluespace sender.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("port")]
        public string PortName { get; set; } = "port";

        /// <summary>
        /// The container name for the gas tank slot.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("container")]
        public string TankContainerName { get; set; } = "tank_slot";

        /// <summary>
        /// The item slot holding the gas tank.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public ItemSlot GasTankSlot { get; set; } = new();

        /// <summary>
        /// The gas mixture stored in Bluespace.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("BluespaceGasMixture")]
        public GasMixture BluespaceGasMixture { get; set; } = new();

        /// <summary>
        /// The gas mixture in the inserted tank.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("tankGasMixture")]
        public GasMixture TankGasMixture { get; set; } = new();

        /// <summary>
        ///     List of bools for retrieving gases
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderRetrieveList")]
        public List<bool> BluespaceVendorRetrieveList { get; set; } = Enumerable.Repeat(false, GasTypeCount).ToList();

        /// <summary>
        /// The target pressure to add to the tank (in percent of 1000kpa).
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("releasePressure")]
        public float ReleasePressure { get; set; } = 0;

        /// <summary>
        /// Whether a Bluespace sender is connected to this vendor.
        /// </summary>
        private bool _bluespaceSenderConnected;
        
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderConnected")]
        public bool BluespaceSenderConnected
        {
            get => _bluespaceSenderConnected;
            set
            {
                if (_bluespaceSenderConnected == value) return;
                _bluespaceSenderConnected = value;
                if (EntitySystem.TryGet(out BluespaceVendorSystem? system))
                {
                    system.OnBluespaceSenderConnected(Owner, this);
                }
            }
        }
    }
}