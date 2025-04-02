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
        /// The minimum pressure for the release valve (in kPa).
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("minReleasePressure")]
        public float MinReleasePressure { get; set; } = 0f;

        /// <summary>
        /// The maximum pressure for the release valve (in kPa).
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("maxReleasePressure")]
        public float MaxReleasePressure { get; set; } = 1000f;

        /// <summary>
        /// The target pressures for each gas type to add to the tank (in kPa).
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("releasePressure")]
        public float[] ReleasePressures { get; set; } = new float[GasTypeCount];

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

        /// <summary>
        /// List of enabled gas types for transfer from the Bluespace sender.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderEnabledList")]
        public List<bool> BluespaceSenderEnabledList { get; set; } = Enumerable.Repeat(false, GasTypeCount).ToList();
    }
}