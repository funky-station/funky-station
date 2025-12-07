// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Harpy
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class HarpySingerComponent : Component
    {
        [DataField("midiActionId", serverOnly: true,
            customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? MidiActionId = "ActionHarpyPlayMidi";

        [DataField("midiAction", serverOnly: true)] // server only, as it uses a server-BUI event !type
        public EntityUid? MidiAction;
    }
}
