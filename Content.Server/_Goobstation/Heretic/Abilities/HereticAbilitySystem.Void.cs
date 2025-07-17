// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Heretic.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Temperature.Components;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using System.Linq;
using Content.Server.Temperature.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Magic;
using Content.Shared.Magic.Events;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    private void SubscribeVoid()
    {
        SubscribeLocalEvent<HereticComponent, HereticAristocratWayEvent>(OnAristocratWay);
        SubscribeLocalEvent<HereticComponent, HereticAscensionVoidEvent>(OnAscensionVoid);

        SubscribeLocalEvent<HereticComponent, HereticVoidBlastEvent>(OnVoidBlast);
        SubscribeLocalEvent<HereticComponent, HereticVoidBlinkEvent>(OnVoidBlink);
        SubscribeLocalEvent<HereticComponent, HereticVoidPullEvent>(OnVoidPull);
    }

    private void OnAristocratWay(Entity<HereticComponent> ent, ref HereticAristocratWayEvent args)
    {
        RemComp<TemperatureComponent>(ent);
        RemComp<TemperatureSpeedComponent>(ent);
        RemComp<RespiratorComponent>(ent);
    }
    private void OnAscensionVoid(Entity<HereticComponent> ent, ref HereticAscensionVoidEvent args)
    {
        RemComp<BarotraumaComponent>(ent);
        EnsureComp<AristocratComponent>(ent);
    }

    private void OnVoidBlast(Entity<HereticComponent> ent, ref HereticVoidBlastEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var rod = Spawn("ImmovableVoidRod", Transform(ent).Coordinates);

        if (TryComp(rod, out PhysicsComponent? phys))
        {
            _phys.SetLinearDamping(rod, phys, 0f);
            _phys.SetFriction(rod, phys, 0f);
            _phys.SetBodyStatus(rod, phys, BodyStatus.InAir);

            var xform = Transform(rod);
            var vel = Transform(ent).WorldRotation.ToWorldVec() * 15f;

            _phys.SetLinearVelocity(rod, vel, body: phys);
            xform.LocalRotation = Transform(ent).LocalRotation;
        }

        args.Handled = true;
    }

    private void OnVoidBlink(Entity<HereticComponent> ent, ref HereticVoidBlinkEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        _aud.PlayPvs(new SoundPathSpecifier("/Audio/_Funkystation/Effects/Heretic/voidblink.ogg"), ent);

        foreach (var pookie in GetNearbyPeople(ent, 2f))
        {
            _stun.TryKnockdown(pookie, TimeSpan.FromSeconds(2f), true);

            if (TryComp<TemperatureComponent>(pookie, out var temp))
                _temperature.ForceChangeTemperature(pookie, temp.CurrentTemperature - 50f, temp);

            if (TryComp<DamageableComponent>(pookie, out var damage))
            {
                var appliedDamageSpecifier =
                    new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(5f));
                _damage.TryChangeDamage(pookie, appliedDamageSpecifier, true, origin: ent);
            }
        }

        _transform.SetCoordinates(ent, args.Target);

        // repeating for both sides
        _aud.PlayPvs(new SoundPathSpecifier("/Audio/_Funkystation/Effects/Heretic/voidblink.ogg"), ent);

        foreach (var pookie in GetNearbyPeople(ent, 2f))
        {
            _stun.TryKnockdown(pookie, TimeSpan.FromSeconds(2f), true);

            if (TryComp<TemperatureComponent>(pookie, out var temp))
                _temperature.ForceChangeTemperature(pookie, temp.CurrentTemperature - 60f, temp);

            if (TryComp<DamageableComponent>(pookie, out var damage))
            {
                var appliedDamageSpecifier =
                    new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(5f));
                _damage.TryChangeDamage(pookie, appliedDamageSpecifier, true, origin: ent);
            }
        }

        args.Handled = true;
    }

    private void OnVoidPull(Entity<HereticComponent> ent, ref HereticVoidPullEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var topPriority = GetNearbyPeople(ent, 2f);
        var midPriority = GetNearbyPeople(ent, 3.5f);
        var farPriority = GetNearbyPeople(ent, 5f);

        // Freeze closest ones, and do heavy damage - funky
        foreach (var pookie in topPriority)
        {
            if (TryComp<TemperatureComponent>(pookie, out var temp))
                _temperature.ForceChangeTemperature(pookie, temp.CurrentTemperature - 100f, temp);

            if (TryComp<DamageableComponent>(pookie, out var damage))
            {
                var appliedDamageSpecifier =
                    new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(25f));
                _damage.TryChangeDamage(pookie, appliedDamageSpecifier, true, origin: ent);
            }
        }

        //Reduce mid-range entities' temps, and do decent damage - funky
        foreach (var pookie in midPriority)
        {
            if (TryComp<TemperatureComponent>(pookie, out var temp))
                _temperature.ForceChangeTemperature(pookie, temp.CurrentTemperature - 60f, temp);

            if (TryComp<DamageableComponent>(pookie, out var damage))
            {
                var appliedDamageSpecifier =
                    new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(12.5f));
                _damage.TryChangeDamage(pookie, appliedDamageSpecifier, true, origin: ent);

                _stun.TryKnockdown(pookie, TimeSpan.FromSeconds(2.5f), true);
            }
        }

        //Do light damage to far ones - funky
        foreach (var pookie in farPriority)
        {
            if (TryComp<DamageableComponent>(pookie, out var damage))
            {
                var appliedDamageSpecifier =
                    new DamageSpecifier(_prot.Index<DamageTypePrototype>("Cold"), FixedPoint2.New(5f));
                _damage.TryChangeDamage(pookie, appliedDamageSpecifier, true, origin: ent);

                _throw.TryThrow(pookie, Transform(ent).Coordinates);
            }
        }

        args.Handled = true;
    }
}
