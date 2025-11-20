// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Amethyst <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kandiyaki <106633914+Kandiyaki@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kit <nikkiestes0@gmail.com>
// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Heretic.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;

namespace Content.Server.Magic;

public sealed partial class ImmovableVoidRodSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we are deliberately including paused entities. rod hungers for all
        foreach (var (rod, trans) in EntityManager.EntityQuery<ImmovableVoidRodComponent, TransformComponent>(true))
        {
            rod.Accumulator += frameTime;

            if (rod.Accumulator > rod.Lifetime.TotalSeconds)
            {
                QueueDel(rod.Owner);
                return;
            }

            if (!_ent.TryGetComponent<MapGridComponent>(trans.GridUid, out var grid))
                continue;



            var tileref = grid.GetTileRef(trans.Coordinates);
            var tile = _prot.Index<ContentTileDefinition>("FloorAstroSnow");
            _tile.ReplaceTile(tileref, tile);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImmovableVoidRodComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<ImmovableVoidRodComponent> ent, ref StartCollideEvent args)
    {
        if ((TryComp<HereticComponent>(args.OtherEntity, out var th) && th.CurrentPath == "Void")
        || HasComp<GhoulComponent>(args.OtherEntity))
            return;

        //_stun.TryParalyze(args.OtherEntity, TimeSpan.FromSeconds(2.5f), false); funky change

        //This is a certified Funkystation addition :fire:

        if (TryComp<TemperatureComponent>(args.OtherEntity, out var temp))
            _temperature.ForceChangeTemperature(args.OtherEntity, temp.CurrentTemperature - 35f, temp);

        if (TryComp<DamageableComponent>(args.OtherEntity, out var damage))
        {
               var appliedDamageSpecifier = new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(12.5f));
            _damage.TryChangeDamage(args.OtherEntity, appliedDamageSpecifier, true, origin: ent);
        }

        TryComp<TagComponent>(args.OtherEntity, out var tag);
        var tags = tag?.Tags ?? new();

        var proto = Prototype(args.OtherEntity);

        if (tags.Contains("Wall") && proto != null && proto.ID != ent.Comp.SnowWallPrototype)
        {
            Spawn(ent.Comp.SnowWallPrototype, Transform(args.OtherEntity).Coordinates);
            QueueDel(args.OtherEntity);
        }
    }
}
