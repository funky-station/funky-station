// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Exists for use as a status effect. Allows surgical operations to not cause immense pain.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AnesthesiaComponent : Component;
