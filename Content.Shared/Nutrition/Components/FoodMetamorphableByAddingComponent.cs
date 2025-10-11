// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Nutrition.Components;

/// <summary>
/// Attempts to metamorphose a modular food when a new ingredient is added.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedFoodSequenceSystem))]
public sealed partial class FoodMetamorphableByAddingComponent : Component
{
    /// <summary>
    /// if true, the metamorphosis will only be attempted when the sequence ends, not when each element is added.
    /// </summary>
    [DataField]
    public bool OnlyFinal = true;
}
