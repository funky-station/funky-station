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
        ///     The port name for connecting to a Bluespace vender.
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
        ///     List of bools for enabled gases
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderEnabledList")]
        public List<bool> BluespaceSenderEnabledList { get; set; } = Enumerable.Repeat(false, GasTypeCount).ToList();

        /// <summary>
        ///     List of bools for retrieving gases
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("bluespaceSenderRetrieveList")]
        public List<bool> BluespaceSenderRetrieveList { get; set; } = Enumerable.Repeat(false, GasTypeCount).ToList();
    }
}