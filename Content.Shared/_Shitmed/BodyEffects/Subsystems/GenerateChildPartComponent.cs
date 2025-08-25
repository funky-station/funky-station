// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.BodyEffects.Subsystems;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GenerateChildPartComponent : Component
{

    [DataField(required: true)]
    public EntProtoId Id = "";

    [DataField, AutoNetworkedField]
    public EntityUid? ChildPart;

    [DataField]
    public bool Active = false;
}