using System.ComponentModel.DataAnnotations;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class PipeBurstComponent  : Component
    {
        public const float MaxExplosionRange = 26f;
        public int Ticker = 0;

        [ViewVariables(VVAccess.ReadWrite), DataField("ruptureSound")]
        //TODO: Change this to something less ass
        public SoundSpecifier RuptureSound = new SoundPathSpecifier("/Audio/Effects/spray.ogg");

        /// <summary>
        ///     Pressure at which pipes start leaking.
        /// </summary>
        [DataField("pipeLeakPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeLeakPressure = 10000;

        /// <summary>
        ///     Pressure at which pipe unanchors.
        /// </summary>
        [DataField("pipeRupturePressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeRupturePressure = 20000;

        /// <summary>
        ///     Pressure at which pipe bursts
        /// </summary>
        [DataField("pipeFragmentPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentPressure = 40000;

        /// <summary>
        ///     Increases explosion for each scale kPa above threshold.
        /// </summary>
        [DataField("pipeFragmentScale"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentScale = 1000;

        /// <summary>
        /// Damage dealt per tick of rupture
        /// </summary>
        [DataField("ruptureDamage"), ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier RuptureDamage = new()
        {
            DamageDict = new()
            {
                { "Structural", 10 },
            }
        };
    }
}

