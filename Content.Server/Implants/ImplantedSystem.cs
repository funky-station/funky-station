// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Components;
using Content.Shared.Implants.Components;
using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.Server.Implants;

public sealed partial class ImplanterSystem
{
    public void InitializeImplanted()
    {
        SubscribeLocalEvent<ImplantedComponent, ComponentInit>(OnImplantedInit);
        SubscribeLocalEvent<ImplantedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ImplantedComponent, BeingGibbedEvent>(OnGibbed);
    }

    private void OnImplantedInit(Entity<ImplantedComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ImplantContainer = _container.EnsureContainer<Container>(ent.Owner, ImplanterComponent.ImplantSlotId);
        ent.Comp.ImplantContainer.OccludesLight = false;
    }

    private void OnShutdown(Entity<ImplantedComponent> ent, ref ComponentShutdown args)
    {
        //If the entity is deleted, get rid of the implants
        _container.CleanContainer(ent.Comp.ImplantContainer);
    }

    private void OnGibbed(Entity<ImplantedComponent> ent, ref BeingGibbedEvent args)
    {
        // Drop the storage implant contents before the implants are deleted by the body being gibbed
        foreach (var implant in ent.Comp.ImplantContainer.ContainedEntities)
        {
            if (TryComp<StorageComponent>(implant, out var storage))
                _container.EmptyContainer(storage.Container, destination: Transform(ent).Coordinates);
        }

    }
}
