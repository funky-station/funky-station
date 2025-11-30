// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System.Linq;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.BloodCult;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Changes blood cultists' blood to Unholy Blood and grants them a Bloodsucker stomach
/// </summary>
public sealed class BloodCultistMetabolismSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    
    // Track blood gland organs by cultist to ensure cleanup
    private readonly Dictionary<EntityUid, EntityUid> _cultistBloodGlands = new();

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BloodCultistComponent, ComponentInit>(OnCultistInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnCultistShutdown);
        // Note: ComponentRemove is handled by BloodCultRuleSystem, so we use ComponentShutdown and EntityTerminatingEvent instead
        SubscribeLocalEvent<BloodCultistComponent, EntityTerminatingEvent>(OnCultistTerminating);
        SubscribeLocalEvent<BodyComponent, EntityTerminatingEvent>(OnBodyTerminating);
        SubscribeLocalEvent<OrganComponent, OrganRemovedFromBodyEvent>(OnOrganRemovedFromBody);
        SubscribeLocalEvent<OrganComponent, ComponentShutdown>(OnOrganShutdown);
        SubscribeLocalEvent<OrganComponent, EntityTerminatingEvent>(OnOrganTerminating);
        
        // Subscribe to BeforeEntityFlush to ensure cleanup before shutdown flush
        EntityManager.BeforeEntityFlush += OnBeforeEntityFlush;
        
        // Subscribe to ContainedSolutionComponent EntityTerminatingEvent for orphaned solution cleanup
        SubscribeLocalEvent<ContainedSolutionComponent, EntityTerminatingEvent>(OnContainedSolutionTerminating);
    }
    
    public override void Shutdown()
    {
        EntityManager.BeforeEntityFlush -= OnBeforeEntityFlush;
        base.Shutdown();
    }

    private void OnCultistInit(EntityUid uid, BloodCultistComponent component, ComponentInit args)
    {
        // Add a blood gland organ (separate from stomach, so we don't interfere with eating)
        // Basically this is a Nar'Sie organ that ensures they always bleed sanguine perniculate
        // There's probably a better way to implement this, so feel free to refactor it away if you figure it out
        if (!TryComp<BodyComponent>(uid, out var body))
            return;
        
        // Check if they already have a blood gland
        bool hasBloodGland = false;
        foreach (var (organUid, organ) in _body.GetBodyOrgans(uid, body))
        {
            if (organ.SlotId == "blood_gland")
            {
                hasBloodGland = true;
                break;
            }
        }
        
        if (!hasBloodGland)
        {
            // Find the torso to add the blood gland
            var parts = _body.GetBodyChildren(uid, body);
            foreach (var (partUid, part) in parts)
            {
                if (part.PartType == BodyPartType.Torso)
                {
                    // Create the blood_gland slot if it doesn't exist
                    if (!part.Organs.ContainsKey("blood_gland"))
                    {
                        _body.TryCreateOrganSlot(partUid, "blood_gland", out _, part);
                    }
                    
                    // Spawn and insert blood gland
                    var coords = Transform(uid).Coordinates;
                    var bloodGland = Spawn("OrganBloodGland", coords);
                    _body.InsertOrgan(partUid, bloodGland, "blood_gland", part);
                    
                    // Track the blood gland for cleanup
                    _cultistBloodGlands[uid] = bloodGland;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Handles cleanup when a body is being terminated.
    /// This fires earlier than ComponentShutdown and ensures blood gland organs are deleted before body parts are deleted.
    /// </summary>
    private void OnBodyTerminating(EntityUid uid, BodyComponent component, ref EntityTerminatingEvent args)
    {
        // Only handle bodies that belong to cultists
        if (!HasComp<BloodCultistComponent>(uid))
            return;

        // Find and delete all blood gland organs in this body
        foreach (var (organUid, organ) in _body.GetBodyOrgans(uid, component))
        {
            if (organ.SlotId == "blood_gland")
            {
                // First, explicitly delete all solution entities in containers
                // Solution entities are stored in containers like "solution@organ" and "solution@food"
                // Try to get solution entities directly by name
                if (_solutionContainer.TryGetSolution((organUid, null), "organ", out var organSolutionEntity))
                {
                    if (Exists(organSolutionEntity.Value.Owner) && !Deleted(organSolutionEntity.Value.Owner))
                        EntityManager.DeleteEntity(organSolutionEntity.Value.Owner);
                }
                if (_solutionContainer.TryGetSolution((organUid, null), "food", out var foodSolutionEntity))
                {
                    if (Exists(foodSolutionEntity.Value.Owner) && !Deleted(foodSolutionEntity.Value.Owner))
                        EntityManager.DeleteEntity(foodSolutionEntity.Value.Owner);
                }
                
                // Also try to get containers directly as a fallback
                var organContainers = _container.GetAllContainers(organUid);
                foreach (var container in organContainers)
                {
                    // Delete all contained entities (solution entities)
                    foreach (var contained in container.ContainedEntities.ToArray())
                    {
                        if (Exists(contained) && !Deleted(contained))
                            EntityManager.DeleteEntity(contained);
                    }
                }
                
                // Remove from container first to prevent orphaning
                _body.RemoveOrgan(organUid);
                
                // Delete the organ immediately (this will recursively delete any remaining child entities)
                if (Exists(organUid) && !Deleted(organUid))
                    EntityManager.DeleteEntity(organUid);
            }
        }
    }

    /// <summary>
    /// Helper method to delete blood gland organ for a cultist.
    /// </summary>
    private void DeleteBloodGlandForCultist(EntityUid cultistUid)
    {
        EntityUid? bloodGlandToDelete = null;

        // Try to get tracked blood gland first
        if (_cultistBloodGlands.TryGetValue(cultistUid, out var trackedGland) && Exists(trackedGland) && !Deleted(trackedGland))
        {
            bloodGlandToDelete = trackedGland;
        }
        
        // If not found in tracking, try to get it through the body system
        if (bloodGlandToDelete == null && TryComp<BodyComponent>(cultistUid, out var body))
        {
            foreach (var (organUid, organ) in _body.GetBodyOrgans(cultistUid, body))
            {
                if (organ.SlotId == "blood_gland")
                {
                    bloodGlandToDelete = organUid;
                    break;
                }
            }
        }

        // If body system failed, search all containers for the organ
        if (bloodGlandToDelete == null)
        {
            var allContainers = _container.GetAllContainers(cultistUid);
            foreach (var container in allContainers)
            {
                foreach (var contained in container.ContainedEntities)
                {
                    if (TryComp<OrganComponent>(contained, out var organ) && organ.SlotId == "blood_gland")
                    {
                        bloodGlandToDelete = contained;
                        break;
                    }
                }
                if (bloodGlandToDelete != null)
                    break;
            }
        }

        // Delete the organ if found
        if (bloodGlandToDelete != null && Exists(bloodGlandToDelete.Value) && !Deleted(bloodGlandToDelete.Value))
        {
            // First, explicitly delete all solution entities in containers
            // Solution entities are stored in containers like "solution@organ" and "solution@food"
            var organContainers = _container.GetAllContainers(bloodGlandToDelete.Value);
            foreach (var container in organContainers)
            {
                // Delete all contained entities (solution entities)
                foreach (var contained in container.ContainedEntities.ToArray())
                {
                    if (Exists(contained) && !Deleted(contained))
                        EntityManager.DeleteEntity(contained);
                }
            }
            
            // Try to remove from container first
            _body.RemoveOrgan(bloodGlandToDelete.Value);
            
            // Delete immediately (this will recursively delete any remaining child entities)
            if (Exists(bloodGlandToDelete.Value) && !Deleted(bloodGlandToDelete.Value))
                EntityManager.DeleteEntity(bloodGlandToDelete.Value);
        }
    }

    /// <summary>
    /// Handles cleanup when a cultist entity is being terminated.
    /// This fires earlier than ComponentShutdown and ensures blood gland organs are deleted before the body is deleted.
    /// </summary>
    private void OnCultistTerminating(EntityUid uid, BloodCultistComponent component, ref EntityTerminatingEvent args)
    {
        // Try to get tracked blood gland first
        EntityUid? bloodGlandToDelete = null;
        if (_cultistBloodGlands.TryGetValue(uid, out var trackedGland) && Exists(trackedGland) && !Deleted(trackedGland))
        {
            bloodGlandToDelete = trackedGland;
        }
        
        // If not found in tracking, try to get it through the body system
        if (bloodGlandToDelete == null && TryComp<BodyComponent>(uid, out var body))
        {
            foreach (var (organUid, organ) in _body.GetBodyOrgans(uid, body))
            {
                if (organ.SlotId == "blood_gland")
                {
                    bloodGlandToDelete = organUid;
                    break;
                }
            }
        }

        // If body system failed, search all containers for the organ
        if (bloodGlandToDelete == null)
        {
            var allContainers = _container.GetAllContainers(uid);
            foreach (var container in allContainers)
            {
                foreach (var contained in container.ContainedEntities)
                {
                    if (TryComp<OrganComponent>(contained, out var organ) && organ.SlotId == "blood_gland")
                    {
                        bloodGlandToDelete = contained;
                        break;
                    }
                }
                if (bloodGlandToDelete != null)
                    break;
            }
        }

        // Delete the organ if found
        if (bloodGlandToDelete != null && Exists(bloodGlandToDelete.Value) && !Deleted(bloodGlandToDelete.Value))
        {
            // First, explicitly delete all solution entities in containers
            // Solution entities are stored in containers like "solution@organ" and "solution@food"
            // Try to get solution entities directly by name
            if (_solutionContainer.TryGetSolution((bloodGlandToDelete.Value, null), "organ", out var organSolutionEntity))
            {
                if (Exists(organSolutionEntity.Value.Owner) && !Deleted(organSolutionEntity.Value.Owner))
                    EntityManager.DeleteEntity(organSolutionEntity.Value.Owner);
            }
            if (_solutionContainer.TryGetSolution((bloodGlandToDelete.Value, null), "food", out var foodSolutionEntity))
            {
                if (Exists(foodSolutionEntity.Value.Owner) && !Deleted(foodSolutionEntity.Value.Owner))
                    EntityManager.DeleteEntity(foodSolutionEntity.Value.Owner);
            }
            
            // Also try to get containers directly as a fallback
            var organContainers = _container.GetAllContainers(bloodGlandToDelete.Value);
            foreach (var container in organContainers)
            {
                // Delete all contained entities (solution entities)
                foreach (var contained in container.ContainedEntities.ToArray())
                {
                    if (Exists(contained) && !Deleted(contained))
                        EntityManager.DeleteEntity(contained);
                }
            }
            
            // Try to remove from container first
            _body.RemoveOrgan(bloodGlandToDelete.Value);
            
            // Delete immediately (this will recursively delete any remaining child entities)
            if (Exists(bloodGlandToDelete.Value) && !Deleted(bloodGlandToDelete.Value))
                EntityManager.DeleteEntity(bloodGlandToDelete.Value);
        }
        
        // Remove from tracking
        _cultistBloodGlands.Remove(uid);
    }

    private void OnCultistShutdown(EntityUid uid, BloodCultistComponent component, ComponentShutdown args)
    {
        // Restore blood type to normal Blood
		if (!TryGetPrototypeBloodReagent(uid, out var restoreReagent))
			restoreReagent = string.Empty;

		if (!string.IsNullOrEmpty(restoreReagent) && TryComp<BloodstreamComponent>(uid, out var bloodstream))
		{
			_bloodstream.ChangeBloodReagent(uid, restoreReagent, bloodstream);
		}

        // Aggressively find and delete the blood gland organ
        // This handles cleanup when the component is removed (ComponentShutdown fires after ComponentRemove)
        DeleteBloodGlandForCultist(uid);
        
        // Remove from tracking
        _cultistBloodGlands.Remove(uid);
    }

    /// <summary>
    /// Handles cleanup when a blood gland organ is removed from a body.
    /// This ensures the organ is properly deleted even if the body/body part is deleted first.
    /// </summary>
    private void OnOrganRemovedFromBody(Entity<OrganComponent> organ, ref OrganRemovedFromBodyEvent args)
    {
        // Only handle blood gland organs
        if (organ.Comp.SlotId != "blood_gland")
            return;

        // Check if this organ belongs to a cultist (by checking if the body has BloodCultistComponent)
        if (args.OldBody.IsValid() && HasComp<BloodCultistComponent>(args.OldBody))
        {
            // Remove from tracking if present
            if (_cultistBloodGlands.ContainsValue(organ))
            {
                var cultist = _cultistBloodGlands.FirstOrDefault(kvp => kvp.Value == organ.Owner).Key;
                if (cultist != EntityUid.Invalid)
                    _cultistBloodGlands.Remove(cultist);
            }
            
            // Delete the organ immediately when removed from body
            // This ensures cleanup happens even if the body is being deleted
            EntityManager.DeleteEntity(organ);
        }
    }

    /// <summary>
    /// Handles cleanup when a blood gland organ's component is shutting down.
    /// This is a last-resort cleanup to ensure the organ is deleted during shutdown.
    /// </summary>
    private void OnOrganShutdown(EntityUid uid, OrganComponent component, ComponentShutdown args)
    {
        // Only handle blood gland organs
        if (component.SlotId != "blood_gland")
            return;

        // If the organ still exists and hasn't been deleted yet, delete it
        // This handles cases where the organ gets orphaned during shutdown
        if (Exists(uid) && !Deleted(uid))
        {
            EntityManager.DeleteEntity(uid);
        }
    }

    /// <summary>
    /// Handles cleanup when a blood gland organ is being terminated.
    /// This fires earlier than ComponentShutdown and ensures cleanup happens during entity deletion.
    /// </summary>
    private void OnOrganTerminating(EntityUid uid, OrganComponent component, ref EntityTerminatingEvent args)
    {
        // Only handle blood gland organs
        if (component.SlotId != "blood_gland")
            return;

        // First, explicitly delete solution entities directly from containers
        // Use direct container access to ensure we get them even if the organ is in a bad state
        if (_container.TryGetContainer(uid, "solution@organ", out var organContainer) 
            && organContainer is ContainerSlot organSlot 
            && organSlot.ContainedEntity is { } organSolution)
        {
            if (Exists(organSolution) && !Deleted(organSolution))
                EntityManager.DeleteEntity(organSolution);
        }
        
        if (_container.TryGetContainer(uid, "solution@food", out var foodContainer) 
            && foodContainer is ContainerSlot foodSlot 
            && foodSlot.ContainedEntity is { } foodSolution)
        {
            if (Exists(foodSolution) && !Deleted(foodSolution))
                EntityManager.DeleteEntity(foodSolution);
        }

        // Ensure the organ is properly removed from its container before deletion
        // This prevents it from being orphaned
        if (component.Body != null && Exists(component.Body.Value))
        {
            // Try to remove the organ from the body if it's still attached
            _body.RemoveOrgan(uid);
        }
    }
    
    /// <summary>
    /// Handles cleanup before entity flush during shutdown.
    /// This ensures all blood gland organs and their solution entities are deleted before the flush.
    /// </summary>
    private void OnBeforeEntityFlush()
    {
        // Find all blood gland organs
        var query = EntityQueryEnumerator<OrganComponent>();
        while (query.MoveNext(out var organUid, out var organ))
        {
            if (organ.SlotId != "blood_gland")
                continue;
                
            // Delete solution entities directly from containers
            if (_container.TryGetContainer(organUid, "solution@organ", out var organContainer) 
                && organContainer is ContainerSlot organSlot 
                && organSlot.ContainedEntity is { } organSolution)
            {
                if (Exists(organSolution) && !Deleted(organSolution))
                    EntityManager.DeleteEntity(organSolution);
            }
            
            if (_container.TryGetContainer(organUid, "solution@food", out var foodContainer) 
                && foodContainer is ContainerSlot foodSlot 
                && foodSlot.ContainedEntity is { } foodSolution)
            {
                if (Exists(foodSolution) && !Deleted(foodSolution))
                    EntityManager.DeleteEntity(foodSolution);
            }
            
            // Delete the organ itself
            if (Exists(organUid) && !Deleted(organUid))
                EntityManager.DeleteEntity(organUid);
        }
    }
    
    /// <summary>
    /// Handles cleanup when a contained solution entity is being terminated.
    /// This ensures solution entities are deleted if their container (blood gland organ) is deleted/terminating.
    /// </summary>
    private void OnContainedSolutionTerminating(EntityUid uid, ContainedSolutionComponent component, ref EntityTerminatingEvent args)
    {
        // Check if the container is a blood gland organ
        if (!TryComp<OrganComponent>(component.Container, out var organ))
            return;
            
        if (organ.SlotId != "blood_gland")
            return;
        
        // If the container (blood gland organ) is deleted/terminating, ensure the solution entity is deleted
        if (TerminatingOrDeleted(component.Container))
        {
            // The solution entity is already being terminated, but we ensure it's deleted
            if (Exists(uid) && !Deleted(uid))
                EntityManager.DeleteEntity(uid);
        }
    }


	private bool TryGetPrototypeBloodReagent(EntityUid uid, out string bloodReagent)
	{
		bloodReagent = string.Empty;

		var meta = MetaData(uid);
		if (meta.EntityPrototype == null)
			return false;

		var componentFactory = IoCManager.Resolve<IComponentFactory>();
		if (!meta.EntityPrototype.TryGetComponent(componentFactory.GetComponentName<BloodstreamComponent>(), out BloodstreamComponent? prototypeBloodstream))
			return false;

		bloodReagent = prototypeBloodstream.BloodReagent;
		return true;
	}
}

