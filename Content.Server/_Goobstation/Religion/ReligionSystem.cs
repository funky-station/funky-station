// SPDX-FileCopyrightText: 2024 Dae <60460608+ZeroDayDaemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

//Public Domain Code - Basically neutered, bibles are in loadouts - Funky
using Content.Server.Bible.Components;
using Content.Server.GameTicking;
using Content.Shared._Goobstation.Religion;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;

namespace Content.Server._Goobstation.Religion;

public sealed class ReligionSystem: EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
    
        if (HasComp<BibleUserComponent>(args.Mob)) //Theoretically this can be used to let everyone spawn with the bible of their chosen faith
        {
            if (EntityManager.TryGetComponent(args.Mob, out ReligionComponent? mobReligion))
            {
                var bible = mobReligion.Type switch
                {
                    Shared._Goobstation.Religion.Religion.Atheist => "BibleAtheist",
                    Shared._Goobstation.Religion.Religion.Buddhist => "BibleBuddhist",
                    Shared._Goobstation.Religion.Religion.Christian => "Bible",
                    Shared._Goobstation.Religion.Religion.None => "Bible",
                };
                _inventorySystem.SpawnItemInSlot(args.Mob, "pocket1", bible, false, false);
            }
        }
    }
}
