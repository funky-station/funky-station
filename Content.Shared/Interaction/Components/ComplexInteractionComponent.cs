// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Interaction.Components;

/// <summary>
/// This is used for identifying entities as being able to use complex interactions with the environment.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedInteractionSystem))]
public sealed partial class ComplexInteractionComponent : Component;
