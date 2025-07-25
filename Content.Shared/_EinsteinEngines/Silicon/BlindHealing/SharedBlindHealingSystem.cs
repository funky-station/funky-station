// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._EinsteinEngines.Silicon.BlindHealing;

public abstract partial class SharedBlindHealingSystem : EntitySystem
{
    [Serializable, NetSerializable]
    protected sealed partial class HealingDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
