// SPDX-FileCopyrightText: 2024 AirFryerBuyOneGetOneFree <jakoblondon01@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Shared._Impstation.CrewMedal;

public abstract class SharedCrewMedalSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrewMedalComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CrewMedalComponent> medal, ref ExaminedEvent args)
    {
        if (!medal.Comp.Awarded)
            return;

        var str = Loc.GetString("comp-crew-medal-inspection-text", ("recipient", medal.Comp.Recipient), ("reason", medal.Comp.Reason));
        args.PushMarkup(str);
    }
}
