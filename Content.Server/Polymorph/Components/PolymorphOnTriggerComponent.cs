// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server.Polymorph.Components;

/// <summary>
/// Intended for use with the trigger system.
/// Polymorphs the user of the trigger.
/// </summary>
[RegisterComponent]
public sealed partial class PolymorphOnTriggerComponent : Component
{
    /// <summary>
    /// Polymorph settings.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> Polymorph;
}
