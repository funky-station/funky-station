// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

[Prototype]
public sealed partial class OreDunGenPrototype : OreDunGen, IPrototype
{
    [IdDataField]
    public string ID { set; get; } = default!;
}
