// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// This is used for clothing that makes an entity weightless when worn.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AntiGravityClothingComponent : Component;
