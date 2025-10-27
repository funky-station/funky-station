// SPDX-FileCopyrightText: 2025 vectorassembly <vectorassembly@icloud.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Traits;
using Content.Shared._Funkystation.Traits.Quirks;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing;
using Content.Shared.Containers;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
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
    [Dependency] private readonly SharedStorageSystem storageSystem = default!;
    // [Dependency] SharedSurgerySystem surgerySystem = default!;
    /// <inheritdoc/>
    //public override void Initialize()
    //{
        //base.Initialize();
        // SubscribeLocalEvent<NitrogenBreathingComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        //this  doesn't work, bullshit^^^

        //SubscribeLocalEvent<NitrogenBreathingComponent, TraitComponentAddedEvent>(TraitComponentAdded);
    //}

    //I am sorry this exists.
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
        {
            if (organ.Component.SlotId == "lungs")
            {
                bodySystem.RemoveOrgan(organ.Id, organ.Component);
                EntityManager.DeleteEntity(organ.Id);
            }
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

        var tankId = EntityManager.Spawn("NitrogenTankFilled");
        var maskId = EntityManager.Spawn("ClothingMaskBreath");

        inventorySystem.TryEquip(playerEntity, tankId, "tankstorage", true, true, false, inventoryComponent);
        inventorySystem.TryEquip(playerEntity, maskId, "mask", true, true, false, inventoryComponent);

        RaiseLocalEvent(playerEntity, new NitrogenBreathingSetup(playerEntity), true);

        inventorySystem.TryGetSlotEntity(playerEntity, "back", out var backpackId, inventoryComponent);
        if (!TryComp(backpackId, out StorageComponent? backpackStorageComponent))
            return;

        foreach (var item in backpackStorageComponent.StoredItems.Keys)
        {
            if (!TryPrototype(item, out var prototype) || !(prototype.ID.Contains("Slots") & prototype.ID.Contains("Box")))
                return;

            EntityManager.DeleteEntity(item);


            //I hate myself for this.
            string boxProto;
            switch (args.SpawnCompleteEvent.JobId)
            {
                case "Mime":
                    boxProto = "BoxMimeSlotsNitrogen";
                    break;

                case "Chemist":
                    boxProto = "BoxSurvivalSlotsMedicalNitrogen";
                    break;
                case "ChiefMedicalOfficer":
                    boxProto = "BoxSurvivalSlotsMedicalNitrogen";
                    break;
                case "MedicalDoctor":
                    boxProto = "BoxSurvivalSlotsMedicalNitrogen";
                    break;
                case "MedicalIntern":
                    boxProto = "BoxSurvivalSlotsMedicalNitrogen";
                    break;
                case "Paramedic":
                    boxProto = "BoxSurvivalSlotsMedicalNitrogen";
                    break;

                case "Detective":
                    boxProto = "BoxSurvivalSlotsSecurityNitrogen";
                    break;
                case "HeadOfSecurity":
                    boxProto = "BoxSurvivalSlotsSecurityNitrogen";
                    break;
                case "SecurityCadet":
                    boxProto = "BoxSurvivalSlotsSecurityNitrogen";
                    break;
                case "SecurityOfficer":
                    boxProto = "BoxSurvivalSlotsSecurityNitrogen";
                    break;
                case "Warden":
                    boxProto = "BoxSurvivalSlotsSecurityNitrogen";
                    break;
                default:
                    boxProto = "BoxSurvivalSlotsNitrogen";
                    break;

            }

            storageSystem.Insert(backpackId.Value, EntityManager.Spawn(boxProto), out _, null, null, false);
        }
    }
}
