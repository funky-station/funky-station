// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics;

public readonly record struct GeneticBlock(int Block, string Sequence)
{
    public static readonly GeneticBlock Invalid = new(-1, string.Empty);
}
