// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Gives a value between the two numbers specified, inclusive.
/// </summary>
public sealed partial class RangeNumberSelector : NumberSelector
{
    [DataField]
    public Vector2i Range = new(1, 1);

    public override int Get(System.Random rand)
    {
        // rand.Next() is inclusive on the first number and exclusive on the second number,
        // so we add 1 to the second number.
        return rand.Next(Range.X, Range.Y + 1);
    }
}
