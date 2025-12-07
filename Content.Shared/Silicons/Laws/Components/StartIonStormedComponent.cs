// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Laws.Components;

/// <summary>
/// Applies law altering ion storms on a specific entity IonStormAmount times when the entity is spawned.
/// </summary>
[RegisterComponent]
public sealed partial class StartIonStormedComponent : Component
{
    /// <summary>
    /// Amount of times that the ion storm will be run on the entity on spawn.
    /// </summary>
    [DataField]
    public int IonStormAmount = 1;
}
