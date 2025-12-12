// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System.Text.Json.Serialization;
using Content.Shared.BloodCult;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.BloodCult.EntityEffects.Effects;

public sealed partial class IsBloodCultistCondition : EntityConditionBase<IsBloodCultistCondition>
{
    [DataField]
    [JsonPropertyName("invert")]
    public bool Invert = false;

    public override bool RaiseEvent(EntityUid uid, IEntityConditionRaiser raiser)
    {
        return raiser.RaiseConditionEvent(uid, this);
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Invert
            ? "Only affects non-cultists."
            : "Only affects blood cultists.";
    }
}
