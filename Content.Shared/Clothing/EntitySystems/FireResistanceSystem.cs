// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Armor;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles reducing fire damage when wearing clothing with <see cref="FireResistanceComponent"/>.
/// </summary>
public sealed class FireResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireResistanceComponent, GetFireResistanceEvent>(OnGetResistance);
        SubscribeLocalEvent<FireResistanceComponent, ArmorExamineEvent>(OnArmorExamine);
    }

    private void OnGetResistance(Entity<FireResistanceComponent> ent, ref GetFireResistanceEvent args)
    {
        args.DamageCoefficient *= ent.Comp.DamageCoefficient;
    }

    private void OnArmorExamine(Entity<FireResistanceComponent> ent, ref ArmorExamineEvent args)
    {
        var value = MathF.Round((1f - ent.Comp.DamageCoefficient) * 100, 1);

        if (value == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.ExamineMessage, ("value", value)));
    }
}
