// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.BloodCult;
using Content.Server.BloodCult;
using Content.Server.BloodCult.EntitySystems;
using Content.Shared.Actions;
using Robust.Server.GameObjects;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// System that handles Blood Cult deconversion when a mindshield is implanted.
/// </summary>
public sealed class BloodCultMindShieldSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to MindShieldComponent being added to avoid duplicate subscriptions with other systems
        SubscribeLocalEvent<MindShieldComponent, ComponentAdd>(OnMindShieldAdded);
    }

    /// <summary>
    /// When a mindshield component is added to an entity, check if they're a cultist and deconvert them.
    /// </summary>
    private void OnMindShieldAdded(EntityUid uid, MindShieldComponent comp, ComponentAdd args)
    {
        // Only process if they're a blood cultist
        if (!HasComp<BloodCultistComponent>(uid))
            return;

        // Stun the cultist as they're deconverted
        var stunTime = TimeSpan.FromSeconds(4);
        var name = Identity.Entity(uid, EntityManager);
        
        // Remove all cult actions/abilities
        _actions.RemoveProvidedActions(uid, uid);
        
        // Remove the cult eyes visual
        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, CultEyesVisuals.CultEyes, false, appearance);
            _appearance.SetData(uid, CultHaloVisuals.CultHalo, false, appearance);
        }
        
        // Remove the cultist component
        RemComp<BloodCultistComponent>(uid);
        
        // Stun them
        _sharedStun.TryParalyze(uid, stunTime, true);
        
        // Show popup message
        _popupSystem.PopupEntity(Loc.GetString("cult-break-control", ("name", name)), uid);

        // Remove the Blood Cult role from their mind
        if (_mindSystem.TryGetMind(uid, out var mindId, out _) &&
            _roleSystem.MindTryRemoveRole<BloodCultRoleComponent>(mindId))
        {
            _adminLogManager.Add(LogType.Mind, LogImpact.Medium, 
                $"{ToPrettyString(uid)} was deconverted from Blood Cult due to being implanted with a Mindshield.");
        }
    }
}

