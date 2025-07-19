// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Prototype("Surgeries")]
public sealed partial class SurgeryComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Priority;

    [DataField, AutoNetworkedField]
    public EntProtoId? Requirement;

    [DataField(required: true), AutoNetworkedField]
    public List<EntProtoId> Steps = new();
}