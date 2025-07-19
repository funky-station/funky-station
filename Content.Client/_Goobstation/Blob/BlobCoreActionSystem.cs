// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Client.Weapons.Melee;
using Content.Shared._Goobstation.Blob;
using Robust.Client.Audio;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Blob;

public sealed class BlobCoreActionSystem : SharedBlobCoreActionSystem
{
    [Dependency] private readonly MeleeWeaponSystem _meleeWeaponSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BlobAttackEvent>(OnBlobAttack);
    }

    [ValidatePrototypeId<EntityPrototype>]
    private const string Animation = "WeaponArcPunch";

    private void OnBlobAttack(BlobAttackEvent ev)
    {
        if(!TryGetEntity(ev.BlobEntity, out var user))
            return;

        _meleeWeaponSystem.DoLunge(user.Value, user.Value, Angle.Zero, ev.Position, Animation, false);
    }
}
