// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 creadth <creadth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Tomeno <Tomeno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Body.Components
{
    [RegisterComponent, Access(typeof(RespiratorSystem))]
    public sealed partial class RespiratorComponent : Component
    {
        /// <summary>
        ///     The next time that this body will inhale or exhale.
        /// </summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan NextUpdate;

        /// <summary>
        ///     The interval between updates. Each update is either inhale or exhale,
        ///     so a full cycle takes twice as long.
        /// </summary>
        [DataField]
        public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

        /// <summary>
        ///     Saturation level. Reduced by UpdateInterval each tick.
        ///     Can be thought of as 'how many seconds you have until you start suffocating' in this configuration.
        /// </summary>
        [DataField]
        public float Saturation = 5.0f;

        /// <summary>
        ///     At what level of saturation will you begin to suffocate?
        /// </summary>
        [DataField]
        public float SuffocationThreshold;

        [DataField]
        public float MaxSaturation = 5.0f;

        [DataField]
        public float MinSaturation = -2.0f;

        // TODO HYPEROXIA?

        [DataField(required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier Damage = default!;

        [DataField(required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier DamageRecovery = default!;

        [DataField]
        public TimeSpan GaspEmoteCooldown = TimeSpan.FromSeconds(8);

        [ViewVariables]
        public TimeSpan LastGaspEmoteTime;

        /// <summary>
        ///     The emote when gasps
        /// </summary>
        [DataField]
        public ProtoId<EmotePrototype> GaspEmote = "Gasp";

        /// <summary>
        ///     How many cycles in a row has the mob been under-saturated?
        /// </summary>
        [ViewVariables]
        public int SuffocationCycles = 0;

        /// <summary>
        ///     How many cycles in a row does it take for the suffocation alert to pop up?
        /// </summary>
        [ViewVariables]
        public int SuffocationCycleThreshold = 3;

        [ViewVariables]
        public RespiratorStatus Status = RespiratorStatus.Inhaling;
    }
}

public enum RespiratorStatus
{
    Inhaling,
    Exhaling
}
