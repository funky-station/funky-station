// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Research.Prototypes;
using Content.Shared.Research.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Research.Components;

/// <summary>
/// This is used for an item that is inserted directly into a given lathe to provide it with a recipe.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BlueprintSystem))]
public sealed partial class BlueprintComponent : Component
{
    /// <summary>
    /// The recipes that this blueprint provides.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<LatheRecipePrototype>> ProvidedRecipes = new();
}
