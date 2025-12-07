// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Atmos.Piping.Components;

/// <summary>
///     Raised directed on an atmos device when it is enabled.
/// </summary>
[ByRefEvent]
public readonly record struct AtmosDeviceDisabledEvent;