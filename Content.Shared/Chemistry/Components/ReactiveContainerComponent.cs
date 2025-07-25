// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Chemistry.Components;

/// <summary>
///     Represents a container that also contains a solution.
///     This means that reactive entities react when inserted into the container.
/// </summary>
[RegisterComponent]
public sealed partial class ReactiveContainerComponent : Component
{
    /// <summary>
    ///     The container that holds the solution.
    /// </summary>
    [DataField(required: true)]
    public string Container = default!;

    /// <summary>
    ///     The solution in the container.
    /// </summary>
    [DataField(required: true)]
    public string Solution = default!;
}
