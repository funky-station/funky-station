// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Magic.Components;

/// <summary>
/// The <see cref="SharedMagicSystem"/> checks this if a spell requires wizard clothes
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedMagicSystem))]
public sealed partial class WizardClothesComponent : Component;
