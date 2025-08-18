// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared.Speech.EntitySystems;

public abstract class SharedSlurredSystem : EntitySystem
{
    public static readonly EntProtoId Stutter = "StatusEffectSlurred";

    public virtual void DoSlur(EntityUid uid, TimeSpan time, StatusEffectsComponent? status = null) { }
}
