// SPDX-FileCopyrightText: 2025 Eris <erisfiregamer1@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using Content.Shared.Changeling;

namespace Content.Server.EntityEffects.Effects;

public sealed class CureChangelingInfectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CureChangelingInfection>(OnCureChangelingInfection);
    }

    private void OnCureChangelingInfection(EntityUid uid, EntityEffectEvent<CureChangelingInfection> args)
    {
        RemCompDeferred<ChangelingInfectionComponent>(uid);
    }
}
