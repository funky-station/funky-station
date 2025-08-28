// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Physics;

/// <summary>
///     A component that allows an entity to have friction (linear and angular dampening)
///     even when not being affected by gravity.
/// </summary>
[RegisterComponent]
public sealed partial class PassiveDampeningComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float LinearDampening = 0.2f;

    [DataField]
    public float AngularDampening = 0.2f;
}
