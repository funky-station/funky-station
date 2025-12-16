// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.EntityConditions.Conditions.Body;

[Serializable]
public sealed partial class AddReagentToBlood : EntityEffectBase<AddReagentToBlood>
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string? Reagent = null;

    [DataField]
    public FixedPoint2 Amount = default!;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (Reagent is not null && prototype.TryIndex(Reagent, out ReagentPrototype? reagentProto))
        {
            return Loc.GetString("reagent-effect-guidebook-add-to-chemicals",
                ("chance", Probability),
                ("deltasign", MathF.Sign(Amount.Float())),
                ("reagent", reagentProto.LocalizedName),
                ("amount", MathF.Abs(Amount.Float())));
        }

        return Loc.GetString("reagent-effect-guidebook-add-to-chemicals-generic");
    }
}
