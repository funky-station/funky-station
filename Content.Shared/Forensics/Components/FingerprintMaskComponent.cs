// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Forensics.Components;

/// <summary>
/// This component stops the entity from leaving fingerprints,
/// usually so fibres can be left instead.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FingerprintMaskComponent : Component;

