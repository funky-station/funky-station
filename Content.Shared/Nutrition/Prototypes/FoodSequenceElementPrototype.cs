// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared.Nutrition.Prototypes;

/// <summary>
/// Unique data storage block for different FoodSequence layers
/// </summary>
[Prototype("foodSequenceElement")]
public sealed partial class FoodSequenceElementPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// sprite options. A random one will be selected and used to display the layer.
    /// </summary>
    [DataField]
    public List<SpriteSpecifier> Sprites { get; private set; } = new();

    /// <summary>
    /// Relative size of the sprite displayed in the food sequence.
    /// </summary>
    [DataField]
    public Vector2 Scale { get; private set; } = Vector2.One;

    /// <summary>
    /// A localized name piece to build into the item name generator.
    /// </summary>
    [DataField]
    public LocId? Name { get; private set; }

    /// <summary>
    /// If the layer is the final one, it can be added over the limit, but no other layers can be added after it.
    /// </summary>
    [DataField]
    public bool Final { get; private set; }

    /// <summary>
    /// Tag list of this layer. Used for recipes for food metamorphosis.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> Tags { get; set; }  = new();
}
