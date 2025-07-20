// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Kitchen.Components;

/// <summary>
///     Applies to items that are capable of butchering entities, or
///     are otherwise sharp for some purpose.
/// </summary>
[RegisterComponent]
public sealed partial class SharpComponent : Component
{
    // TODO just make this a tool type.
    public HashSet<EntityUid> Butchering = new();

    [DataField("butcherDelayModifier")]
    public float ButcherDelayModifier = 1.0f;
}
