// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Roles
{
    [Prototype]
    public sealed class AntagCategoryPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; set; } = string.Empty;
        [DataField]
        public string Name { get; set; } = string.Empty;
    }
}
