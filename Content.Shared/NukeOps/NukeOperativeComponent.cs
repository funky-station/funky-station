// SPDX-FileCopyrightText: 2023 IntegerTempest <30300017+IntegerTempest@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.NukeOps;

/// <summary>
/// This is used for tagging a mob as a nuke operative.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NukeOperativeComponent : Component
{

    /// <summary>
    ///
    /// </summary>
    [DataField("syndStatusIcon", customTypeSerializer: typeof(PrototypeIdSerializer<FactionIconPrototype>))]
    public string SyndStatusIcon = "SyndicateFaction";
}
