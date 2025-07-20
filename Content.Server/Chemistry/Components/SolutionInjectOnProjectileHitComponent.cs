// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Chemistry.Components;

/// <summary>
/// Used for projectile entities that should try to inject a
/// contained solution into a target when they hit it.
/// </summary>
[RegisterComponent]
public sealed partial class SolutionInjectOnProjectileHitComponent : BaseSolutionInjectOnEventComponent { }
