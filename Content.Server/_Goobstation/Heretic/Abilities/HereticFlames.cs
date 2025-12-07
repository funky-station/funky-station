// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server.Heretic.Abilities;

[RegisterComponent]
public sealed partial class HereticFlamesComponent : Component
{
    public float Timer = 0f;
    public float TimerSeconds = 0f;
    public float UpdateDuration = .2f;
    [DataField] public float Duration = 60f;
}

public sealed partial class HereticFlamesSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var hfc in EntityQuery<HereticFlamesComponent>())
        {
            hfc.Timer += frameTime;
            if (hfc.Timer < hfc.UpdateDuration)
                continue;

            hfc.Timer = 0f;
            hfc.TimerSeconds += 1f;

            if (hfc.TimerSeconds >= hfc.Duration)
                RemComp(hfc.Owner, hfc);

            var gasmix = _atmos.GetTileMixture((hfc.Owner, Transform(hfc.Owner)));

            if (gasmix == null)
                continue;

            gasmix.AdjustMoles(Gas.Plasma, 2f);
            gasmix.Temperature = Atmospherics.T0C + 125f;
        }
    }
}
