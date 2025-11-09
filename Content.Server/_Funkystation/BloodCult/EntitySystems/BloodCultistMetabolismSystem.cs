// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.BloodCult;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

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
                    break;
                }
            }
        }
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

	private bool TryGetPrototypeBloodReagent(EntityUid uid, out string bloodReagent)
	{
		bloodReagent = string.Empty;

		if (!TryComp<MetaDataComponent>(uid, out var meta) || meta.EntityPrototype == null)
			return false;

		var componentFactory = IoCManager.Resolve<IComponentFactory>();
		if (!meta.EntityPrototype.TryGetComponent(componentFactory.GetComponentName<BloodstreamComponent>(), out BloodstreamComponent? prototypeBloodstream))
			return false;

		bloodReagent = prototypeBloodstream.BloodReagent;
		return true;
	}
}

