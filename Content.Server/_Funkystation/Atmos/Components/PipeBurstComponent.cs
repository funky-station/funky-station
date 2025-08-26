using Content.Shared.Atmos;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class PipeBurstComponent  : Component
    {
        public const float MaxExplosionRange = 26f;

        [ViewVariables(VVAccess.ReadWrite), DataField("ruptureSound")]
        public SoundSpecifier RuptureSound = new SoundPathSpecifier("/Audio/Effects/spray.ogg");

        /// <summary>
        ///     Pressure at which pipes start leaking.
        /// </summary>
        [DataField("pipeLeakPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeLeakPressure = 90 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Pressure at which pipe unanchors.
        /// </summary>
        [DataField("pipeRupturePressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeRupturePressure = 120 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Pressure at which pipe bursts
        /// </summary>
        [DataField("pipeFragmentPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentPressure = 150 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Increases explosion for each scale kPa above threshold.
        /// </summary>
        [DataField("pipeFragmentScale"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentScale = 2 * Atmospherics.OneAtmosphere;

        /// <summary>
        /// Damage dealt per tick of rupture
        /// </summary>
        [DataField("ruptureDamage"), ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier RuptureDamage = new()
        {
            DamageDict = new()
            {
                { "Structural", 1 },
            }
        };


    }
}

