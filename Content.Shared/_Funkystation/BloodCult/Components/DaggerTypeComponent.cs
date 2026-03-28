// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Component that marks a blood cult dagger for upgrading purposes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DaggerTypeComponent : Component
{
    /// <summary>
    /// The type of dagger.
    /// </summary>
    [DataField]
	public string Type;
}

