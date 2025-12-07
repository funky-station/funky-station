// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT


namespace Content.Shared.Fax.Components;

/// <summary>
/// Event for killing any mob within the fax machine.
/// </summary
[ByRefEvent]
public record struct DamageOnFaxecuteEvent(FaxMachineComponent? Action);

