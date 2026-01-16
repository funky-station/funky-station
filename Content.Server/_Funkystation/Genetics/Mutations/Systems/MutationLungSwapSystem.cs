using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Body.Components;
using Content.Shared.Body.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationLungSwapSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private const string HiddenStorageContainerId = "mutation_hidden_lung_storage";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationLungSwapComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MutationLungSwapComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<MutationLungSwapComponent> ent, ref ComponentStartup args)
    {
        var comp = ent.Comp;

        // Find current lung
        if (!TryGetLungOrgan(ent.Owner, out var originalLungNullable) ||
            originalLungNullable is not { } originalLung)
        {
            RemComp<MutationLungSwapComponent>(ent.Owner);
            return;
        }

        // Find the lung slot in torso
        if (!TryGetLungSlot(ent.Owner, out var lungSlot) || lungSlot is null)
        {
            RemComp<MutationLungSwapComponent>(ent.Owner);
            return;
        }

        comp.OriginalLung = originalLung;

        // Remove original from body
        _container.Remove(originalLung, lungSlot);

        // Create hidden storage and stash original
        var hiddenContainer = _container.EnsureContainer<ContainerSlot>(ent.Owner, HiddenStorageContainerId);
        _container.Insert(originalLung, hiddenContainer);

        // Spawn new lung
        var newLung = Spawn(comp.NewLungPrototype, Transform(ent.Owner).Coordinates);
        comp.SwappedLung = newLung;

        // Insert new lung
        _container.Insert(newLung, lungSlot);
    }

    private void OnShutdown(Entity<MutationLungSwapComponent> ent, ref ComponentShutdown args)
    {
        var comp = ent.Comp;

        if (comp.OriginalLung is not { Valid: true } original ||
            comp.SwappedLung is not { Valid: true } swapped)
            return;

        if (!TryGetLungSlot(ent.Owner, out var lungSlot) || lungSlot is null)
            return;

        // Remove current (mutated) lung
        if (lungSlot.ContainedEntity is { } current)
        {
            _container.Remove(current, lungSlot);
            Del(current);
        }

        // Retrieve original from hidden storage
        if (_container.TryGetContainer(ent.Owner, HiddenStorageContainerId, out var baseHiddenContainer) &&
            baseHiddenContainer is ContainerSlot hiddenContainer &&
            hiddenContainer.ContainedEntity is { } storedLung)
        {
            _container.Remove(storedLung, hiddenContainer);
            _container.Insert(storedLung, lungSlot);
        }

        comp.OriginalLung = null;
        comp.SwappedLung = null;

        // Clean up empty hidden container
        if (_container.TryGetContainer(ent.Owner, HiddenStorageContainerId, out var cleanupBase) &&
            cleanupBase is ContainerSlot cleanupSlot &&
            cleanupSlot.ContainedEntity is null)
        {
            _container.ShutdownContainer(cleanupSlot);
        }
    }

    private bool TryGetLungOrgan(EntityUid body, out EntityUid? lung)
    {
        lung = null;

        foreach (var (organUid, _) in _body.GetBodyOrgans(body))
        {
            if (HasComp<LungComponent>(organUid))
            {
                lung = organUid;
                return true;
            }
        }

        return false;
    }

    private bool TryGetLungSlot(EntityUid body, out ContainerSlot? slot)
    {
        slot = null;

        foreach (var (partId, _) in _body.GetBodyChildren(body))
        {
            if (_container.TryGetContainer(partId, "body_organ_slot_lungs", out var container) &&
                container is ContainerSlot organSlot)
            {
                slot = organSlot;
                return true;
            }
        }

        return false;
    }
}
