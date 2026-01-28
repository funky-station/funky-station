// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationAdrenalineRushComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ActionId = "ActionAdrenalineRush";

    public EntityUid? GrantedAction;
}
