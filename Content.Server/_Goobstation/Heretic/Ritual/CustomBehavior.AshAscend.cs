// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualAshAscendBehavior : RitualSacrificeBehavior
{
    private List<EntityUid> burningUids = new();

    // check for burning corpses
    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        for (int i = 0; i < Max; i++)
        {
            if (args.EntityManager.TryGetComponent<FlammableComponent>(uids[i], out var flam))
                if (flam.OnFire)
                    burningUids.Add(uids[i]);
        }

        if (burningUids.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-ash");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        var damageableSystem = args.EntityManager.System<DamageableSystem>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        for (var i = 0; i < Max; i++)
        {
            // YES!!! ASH!!!
            if (args.EntityManager.TryGetComponent<DamageableComponent>(uids[i], out var dmg))
            {
                var prot = (ProtoId<DamageGroupPrototype>) "Burn";
                var dmgtype = prototypeManager.Index(prot);
                damageableSystem.TryChangeDamage(uids[i], new DamageSpecifier(dmgtype, 3984f), true);
            }
        }

        // reset it because blehhh
        uids = new();
    }
}
