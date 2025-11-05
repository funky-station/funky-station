// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Revolutionary.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedRevolutionarySystem))]
public sealed partial class RevolutionaryLieutenantComponent : Component
{
    /// <summary>
    /// The status icon corresponding to a lieutenant.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "RevolutionaryLieutenantFaction";
    public override bool SessionSpecific => true;
}