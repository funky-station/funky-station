// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Body.Systems;
using Content.Shared.BloodCult;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Changes blood cultists' blood to Unholy Blood and grants them a Bloodsucker stomach
/// </summary>
public sealed class BloodCultistMetabolismSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BloodCultistComponent, ComponentInit>(OnCultistInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnCultistShutdown);
    }

    private void OnCultistInit(EntityUid uid, BloodCultistComponent component, ComponentInit args)
    {
        // Change blood type to Sanguine Perniculate
        _bloodstream.ChangeBloodReagent(uid, "SanguinePerniculate");
        
        // Add a blood gland organ (separate from stomach, so we don't interfere with eating)
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
                    break;
                }
            }
        }
    }

    private void OnCultistShutdown(EntityUid uid, BloodCultistComponent component, ComponentShutdown args)
    {
        // Restore blood type to normal Blood
        _bloodstream.ChangeBloodReagent(uid, "Blood");
        
        // Remove the blood gland organ if it exists
        if (!TryComp<BodyComponent>(uid, out var body))
            return;
        
        // Find and remove the blood gland
        foreach (var (organUid, organ) in _body.GetBodyOrgans(uid, body))
        {
            if (organ.SlotId == "blood_gland")
            {
                // Remove the blood gland organ
                _body.RemoveOrgan(organUid);
                QueueDel(organUid);
                break;
            }
        }
    }
}

