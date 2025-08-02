// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ThunderBear2006 <100388962+ThunderBear2006@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.Atmos.Effects;

/// <summary>
/// This component is used for adjusting the temperature of an entities surrounding tile.
/// </summary>
[RegisterComponent]
public sealed partial class TemperatureAdjustingComponent : Component
{

    /// <summary>
    /// The amount the tempurature should be modified by (negative for decreasing temp)
    /// </summary>
    [DataField("tempChangePerSecond")]
    public float TempChangePerSecond = 0;

    /// <summary>
    /// The maximum temperature that the entity can affect
    /// </summary>
    [DataField("maxTemperature")]
    public float MaxTemperature = 0;

    /// <summary>
    /// The minimum temperature that the entity can affect
    /// </summary>
    [DataField("minTemperature")]
    public float MinTemperature = 0;
}
