// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets this objective's target to the one given in <see cref="TargetOverrideComponent"/>, if the entity has it.
/// This component needs to be added to objective entity itself.
/// </summary>
[RegisterComponent]
public sealed partial class PickSpecificPersonComponent : Component;
