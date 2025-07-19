// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Humanoid;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences
{
    public interface ICharacterProfile
    {
        string Name { get; }

        ICharacterAppearance CharacterAppearance { get; }

        bool MemberwiseEquals(ICharacterProfile other);

        /// <summary>
        ///     Makes this profile valid so there's no bad data like negative ages.
        /// </summary>
        void EnsureValid(ICommonSession session, IDependencyCollection collection);

        /// <summary>
        /// Gets a copy of this profile that has <see cref="EnsureValid"/> applied, i.e. no invalid data.
        /// </summary>
        ICharacterProfile Validated(ICommonSession session, IDependencyCollection collection);
    }
}
