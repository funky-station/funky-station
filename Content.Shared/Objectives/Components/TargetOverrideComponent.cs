// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets a target objective to a specific target when receiving it.
/// The objective entity needs to have <see cref="PickSpecificPersonComponent"/>.
/// This component needs to be added to entity receiving the objective.
/// </summary>
[RegisterComponent]
public sealed partial class TargetOverrideComponent : Component
{
    /// <summary>
    /// The entity that should be targeted.
    /// </summary>
    [DataField]
    public EntityUid? Target;
}
