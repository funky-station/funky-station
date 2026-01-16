// SPDX-FileCopyrightText: 2026 Zergologist <114537969+Chedd-Error@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreMinorBodilyAlertsComponent : Component
{

    //[DataField]
    //public ProtoId<AlertCategoryPrototype> ThirstyCategory = "Thirst";

    public static readonly Dictionary<ThirstThreshold, ProtoId<AlertPrototype>> ThirstThresholdAlertTypes = new()
    {
        {ThirstThreshold.Parched, "Parched"},
        {ThirstThreshold.Dead, "Parched"},
    };

    //[DataField]
    //public ProtoId<AlertCategoryPrototype> HungerAlertCategory = "Hunger";

    public static readonly Dictionary<HungerThreshold, ProtoId<AlertPrototype>> HungerThresholdAlerts = new()
    {
        { HungerThreshold.Starving, "Starving" },
        { HungerThreshold.Dead, "Starving" }

    };
}
