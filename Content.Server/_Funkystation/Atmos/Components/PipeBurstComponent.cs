// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
        public float PipeLeakPressure = 2 * Atmospherics.MaxOutputPressure;

        /// <summary>
        ///     Pressure at which pipe unanchors.
        /// </summary>
        [DataField("pipeRupturePressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeRupturePressure = 3 * Atmospherics.MaxOutputPressure;

        /// <summary>
        ///     Pressure at which pipe bursts
        /// </summary>
        [DataField("pipeFragmentPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentPressure = 4 * Atmospherics.MaxOutputPressure;

        /// <summary>
        ///     Increases explosion for each scale kPa above threshold.
        /// </summary>
        [DataField("pipeFragmentScale"), ViewVariables(VVAccess.ReadWrite)]
        public float PipeFragmentScale = 2 * Atmospherics.MaxOutputPressure;

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

