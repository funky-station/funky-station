// SPDX-FileCopyrightText: 2024 fox <daytimer253@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._Floof.Traits.Components;

[RegisterComponent]
public sealed partial class FixtureDensityModifierComponent : Component
{
    /// <summary>
    ///     The minimum and maximum density that may be used as input for and achieved as a result of application of this component.
    /// </summary>
    [DataField]
    public float Min = float.Epsilon, Max = float.PositiveInfinity;

    [DataField]
    public float Factor = 1f;
}
