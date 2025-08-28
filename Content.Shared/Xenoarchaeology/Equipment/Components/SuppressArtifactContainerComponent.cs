// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Xenoarchaeology.Equipment.Components;

/// <summary>
///     Suppress artifact activation, when entity is placed inside this container.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuppressArtifactContainerSystem))]
public sealed partial class SuppressArtifactContainerComponent : Component;
