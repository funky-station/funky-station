// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Research.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Research.Components;

/// <summary>
/// This is used for a lathe that can utilize <see cref="BlueprintComponent"/>s to gain more recipes.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BlueprintSystem))]
public sealed partial class BlueprintReceiverComponent : Component
{
    [DataField]
    public string ContainerId = "blueprint";

    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}
