// SPDX-FileCopyrightText: 2025 ImpstationBot <irismessage+bot@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._EE.Carrying;

/// <summary>
///     Entities with this component will override the number of free hands required to carry an entity, always requiring one hand instead.
///     Used primarily for entities which only have one hand, but still need to be able to carry.
/// </summary>
[RegisterComponent, Access(typeof(CarryingSystem))]
public sealed partial class CarrierOneHandComponent : Component { }
