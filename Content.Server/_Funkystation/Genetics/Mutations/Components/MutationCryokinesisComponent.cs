// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationCryokinesisComponent : Component
{
    [DataField]
    public float Cooldown = 25f;

    public TimeSpan NextUse = TimeSpan.Zero;

    public EntityUid? GrantedAction;
}
