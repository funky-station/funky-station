using Content.Shared.Atmos;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class PipeBurstComponent  : Component
    {

        /// <summary>
        ///     Pressure at which pipes start leaking.
        /// </summary>
        [DataField("pipeLeakPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeLeakPressure = 30 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Pressure at which pipe unanchors.
        /// </summary>
        [DataField("pipeRupturePressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeRupturePressure = 40 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Pressure at which pipe bursts
        /// </summary>
        [DataField("pipeFragmentPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentPressure = 50 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Increases explosion for each scale kPa above threshold.
        /// </summary>
        [DataField("pipeFragmentScale"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentScale = 2 * Atmospherics.OneAtmosphere;

    }
}

