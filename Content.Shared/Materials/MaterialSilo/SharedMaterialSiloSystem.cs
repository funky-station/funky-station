// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Power.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared.Materials.MaterialSilo;

public abstract class SharedMaterialSiloSystem : EntitySystem
{
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MaterialSiloClientComponent> _clientQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MaterialSiloComponent, ToggleMaterialSiloClientMessage>(OnToggleMaterialSiloClient);
        SubscribeLocalEvent<MaterialSiloComponent, ComponentShutdown>(OnSiloShutdown);
        Subs.BuiEvents<MaterialSiloComponent>(MaterialSiloUiKey.Key,
            subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnBoundUIOpened);
        });


        SubscribeLocalEvent<MaterialSiloClientComponent, GetStoredMaterialsEvent>(OnGetStoredMaterials);
        SubscribeLocalEvent<MaterialSiloClientComponent, ConsumeStoredMaterialsEvent>(OnConsumeStoredMaterials);
        SubscribeLocalEvent<MaterialSiloClientComponent, ComponentShutdown>(OnClientShutdown);

        _clientQuery = GetEntityQuery<MaterialSiloClientComponent>();
    }

    private void OnToggleMaterialSiloClient(Entity<MaterialSiloComponent> ent, ref ToggleMaterialSiloClientMessage args)
    {
        var client = GetEntity(args.Client);

        if (!_clientQuery.TryComp(client, out var clientComp))
            return;

        if (ent.Comp.Clients.Contains(client)) // remove client
        {
            clientComp.Silo = null;
            Dirty(client, clientComp);
            ent.Comp.Clients.Remove(client);
            Dirty(ent);

            UpdateMaterialSiloUi(ent);
        }
        else // add client
        {
            if (!CanTransmitMaterials((ent, ent), client))
                return;

            var clientMats = _materialStorage.GetStoredMaterials(client, true);
            var inverseMats = new Dictionary<string, int>();
            foreach (var (mat, amount) in clientMats)
            {
                inverseMats.Add(mat, -amount);
            }
            _materialStorage.TryChangeMaterialAmount(client, inverseMats, localOnly: true);
            _materialStorage.TryChangeMaterialAmount(ent.Owner, clientMats);

            ent.Comp.Clients.Add(client);
            Dirty(ent);
            clientComp.Silo = ent;
            Dirty(client, clientComp);

            UpdateMaterialSiloUi(ent);
        }
    }

    private void OnBoundUIOpened(Entity<MaterialSiloComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateMaterialSiloUi(ent);
    }

    private void OnSiloShutdown(Entity<MaterialSiloComponent> ent, ref ComponentShutdown args)
    {
        foreach (var client in ent.Comp.Clients)
        {
            if (!_clientQuery.TryComp(client, out var comp))
                continue;

            comp.Silo = null;
            Dirty(client, comp);
        }
    }

    protected virtual void UpdateMaterialSiloUi(Entity<MaterialSiloComponent> ent)
    {

    }

    private void OnGetStoredMaterials(Entity<MaterialSiloClientComponent> ent, ref GetStoredMaterialsEvent args)
    {
        if (args.LocalOnly)
            return;

        if (ent.Comp.Silo is not { } silo)
            return;

        if (!CanTransmitMaterials(silo, ent))
            return;

        var materials = _materialStorage.GetStoredMaterials(silo);

        foreach (var (mat, amount) in materials)
        {
            // Don't supply materials that they don't usually have access to.
            if (!_materialStorage.IsMaterialWhitelisted((args.Entity, args.Entity), mat))
                continue;

            var existing = args.Materials.GetOrNew(mat);
            args.Materials[mat] = existing + amount;
        }
    }

    private void OnConsumeStoredMaterials(Entity<MaterialSiloClientComponent> ent, ref ConsumeStoredMaterialsEvent args)
    {
        if (args.LocalOnly)
            return;

        if (ent.Comp.Silo is not { } silo || !TryComp<MaterialStorageComponent>(silo, out var materialStorage))
            return;

        if (!CanTransmitMaterials(silo, ent))
            return;

        foreach (var (mat, amount) in args.Materials)
        {
            if (!_materialStorage.TryChangeMaterialAmount(silo, mat, amount, materialStorage))
                continue;
            args.Materials[mat] = 0;
        }
    }

    private void OnClientShutdown(Entity<MaterialSiloClientComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MaterialSiloComponent>(ent.Comp.Silo, out var silo))
            return;

        silo.Clients.Remove(ent);
        Dirty(ent.Comp.Silo.Value, silo);
        UpdateMaterialSiloUi((ent.Comp.Silo.Value, silo));
    }

    /// <summary>
    /// Checks if a given client fulfills the criteria to link/receive materials from an ore silo.
    /// </summary>
    [PublicAPI]
    public bool CanTransmitMaterials(Entity<MaterialSiloComponent?> silo, EntityUid client)
    {
        if (!Resolve(silo, ref silo.Comp))
            return false;

        if (!_powerReceiver.IsPowered(silo.Owner))
            return false;

        if (_transform.GetGrid(client) != _transform.GetGrid(silo.Owner))
            return false;

        if (!_transform.InRange(silo.Owner, client, silo.Comp.Range))
            return false;

        return true;
    }
}
