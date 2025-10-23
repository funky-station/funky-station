using Content.Shared._Funkystation.Traits;
using Content.Shared._Funkystation.Traits.Quirks;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Quirks;

/// <summary>
/// This handles...
/// </summary>
public sealed class NitrogenBreathingSystem : EntitySystem
{
    // [Dependency] SharedSurgerySystem surgerySystem = default!;
    [Dependency] private readonly IPrototypeManager prototypeManager = default!;
    [Dependency] private readonly SharedBodySystem bodySystem = default!;
    [Dependency] private readonly LoadoutSystem loadoutSystem = default!;
    [Dependency] private readonly InventorySystem inventorySystem = default!;
    // [Dependency] SharedSurgerySystem surgerySystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        // SubscribeLocalEvent<NitrogenBreathingComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        //this  doesn't work, bullshit^^^

        SubscribeLocalEvent<NitrogenBreathingComponent, TraitComponentAddedEvent>(TraitComponentAdded);
    }

    private void TraitComponentAdded(EntityUid playerEntity, NitrogenBreathingComponent component, TraitComponentAddedEvent args)
    {
        if (!TryComp(playerEntity, out BodyComponent? bodyComponent)) return;
        (EntityUid bodyPartEntity, BodyPartComponent bodyPart)? bodyRoot = bodySystem.GetRootPartOrNull(playerEntity, bodyComponent);
        if (bodyRoot == null || bodyRoot.Value.bodyPart.PartType != BodyPartType.Torso)
            return;
        var torso = bodyRoot.Value.bodyPart;
        if (torso.Body == null)
            return;

        var organs = bodySystem.GetPartOrgans(bodyRoot.Value.bodyPartEntity, torso);
        foreach (var organ in organs)
            if (organ.Component.SlotId == "lungs")
            {
                bodySystem.RemoveOrgan(organ.Id, organ.Component);
                EntityManager.DeleteEntity(organ.Id);
            }

        var newLungsId = EntityManager.Spawn("OrganVoxLungs");
        TryComp(newLungsId, out OrganComponent? newLungs);
        bodySystem.InsertOrgan(bodyRoot.Value.bodyPartEntity, newLungsId, "lungs", bodyRoot.Value.bodyPart, newLungs);


        if (!TryComp(playerEntity, out InventoryComponent? inventoryComponent))
            return;
        if (!inventorySystem.TryGetSlot(playerEntity, "tankstorage", out SlotDefinition? tankSlot, inventoryComponent))
            return;

        inventorySystem.SetSlotIgnoreDependencices(playerEntity, inventoryComponent, "tankstorage");

        Dirty(playerEntity, inventoryComponent);

        var tankId = EntityManager.Spawn("NitrogenTank");
        var maskId = EntityManager.Spawn("ClothingMaskBreath");
        inventorySystem.TryEquip(playerEntity, tankId, "tankstorage", true, true, false, inventoryComponent);
        inventorySystem.TryEquip(playerEntity, maskId, "mask", true, true, false, inventoryComponent);

        RaiseLocalEvent(playerEntity, new NitrogenBreathingSetup(playerEntity));
        // TryComp(tankId, out Internals)
        // inventorySystem.mo
    }
}
