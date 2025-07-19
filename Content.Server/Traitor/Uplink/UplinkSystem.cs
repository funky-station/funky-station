// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Store.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Traitor.Uplink
{
    public sealed class UplinkSystem : EntitySystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly StoreSystem _store = default!;

        [ValidatePrototypeId<CurrencyPrototype>]
        public const string TelecrystalCurrencyPrototype = "Telecrystal";

        /// <summary>
        /// Adds an uplink to the target
        /// </summary>
        /// <param name="user">The person who is getting the uplink</param>
        /// <param name="balance">The amount of currency on the uplink. If null, will just use the amount specified in the preset.</param>
        /// <param name="currencyProtoId">Id of the currency the store uses. If null, uses Telecrystal</param>
        /// <param name="storePreset">If set to a value, will clear the original preset and replace it with this one.</param>
        /// <param name="uplinkEntity">The entity that will actually have the uplink functionality. Defaults to the PDA if null.</param>
        /// <returns>Whether or not the uplink was added successfully</returns>
        public bool AddUplink(EntityUid user, FixedPoint2? balance, EntProtoId? currencyProtoId, EntProtoId? storePreset, EntityUid? uplinkEntity = null)
        {
            // Try to find target item
            if (uplinkEntity == null)
            {
                uplinkEntity = FindUplinkTarget(user);
                if (uplinkEntity == null)
                    return false;
            }

            if (storePreset != null && HasComp<StoreComponent>(uplinkEntity.Value))
            {
                EnsureComp<StoreComponent>(uplinkEntity.Value, out var uplinkPdaStore);

                var ent = Spawn(storePreset);
                if (!TryComp<StoreComponent>(ent, out var comp))
                    return false;

                // there must be a better way of doing this.
                // lmk if there is . . .
                uplinkPdaStore.Categories = comp.Categories;
                uplinkPdaStore.CurrencyWhitelist = comp.CurrencyWhitelist;
                uplinkPdaStore.Name = comp.Name;
                uplinkPdaStore.RefundAllowed = comp.RefundAllowed;

                QueueDel(ent);
            }

            EnsureComp<UplinkComponent>(uplinkEntity.Value);
            var store = EnsureComp<StoreComponent>(uplinkEntity.Value); // so this is why every pda has StorePresetUplink
            store.AccountOwner = user;
            store.Balance.Clear();
            if (balance != null)
            {
                store.Balance.Clear();
                _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { currencyProtoId ?? TelecrystalCurrencyPrototype, balance.Value } }, uplinkEntity.Value, store);
            }

            // TODO add BUI. Currently can't be done outside of yaml -_-

            return true;
        }

        // funkystation
        public bool AddUplink(EntityUid user, FixedPoint2? balance, EntityUid? uplinkEntity = null)
        {
            return AddUplink(user, balance, null, null, uplinkEntity);
        }

        /// <summary>
        /// Finds the entity that can hold an uplink for a user.
        /// Usually this is a pda in their pda slot, but can also be in their hands. (but not pockets or inside bag, etc.)
        /// </summary>
        public EntityUid? FindUplinkTarget(EntityUid user)
        {
            // Try to find PDA in inventory
            if (_inventorySystem.TryGetContainerSlotEnumerator(user, out var containerSlotEnumerator))
            {
                while (containerSlotEnumerator.MoveNext(out var pdaUid))
                {
                    if (!pdaUid.ContainedEntity.HasValue) continue;

                    if (HasComp<PdaComponent>(pdaUid.ContainedEntity.Value) || HasComp<StoreComponent>(pdaUid.ContainedEntity.Value))
                        return pdaUid.ContainedEntity.Value;
                }
            }

            // Also check hands
            foreach (var item in _handsSystem.EnumerateHeld(user))
            {
                if (HasComp<PdaComponent>(item) || HasComp<StoreComponent>(item))
                    return item;
            }

            return null;
        }
    }
}
