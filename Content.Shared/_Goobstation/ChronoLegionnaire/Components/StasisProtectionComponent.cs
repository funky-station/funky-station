// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._Goobstation.ChronoLegionnaire.Components
{
    /// <summary>
    /// Marks entity (clothing) that will give stasis immunity to wearer
    /// </summary>
    [RegisterComponent]
    public sealed partial class StasisProtectionComponent : Component
    {
        /// <summary>
        /// Stamina buff to entity wearer (until stun resist will be added)
        /// </summary>
        [DataField]
        public float StaminaModifier = 10f;
    }
}
