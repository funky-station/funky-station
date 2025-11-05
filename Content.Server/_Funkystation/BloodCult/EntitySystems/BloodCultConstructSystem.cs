// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameObjects;
using Content.Server.Mind;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.DragDrop;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Content.Shared.Damage;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class BloodCultConstructSystem : EntitySystem
{
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;
	[Dependency] private readonly SharedContainerSystem _container = default!;

	public override void Initialize()
	{
		base.Initialize();
		
		SubscribeLocalEvent<BloodCultConstructShellComponent, CanDropTargetEvent>(OnCanDropTarget);
		SubscribeLocalEvent<BloodCultConstructShellComponent, DragDropTargetEvent>(OnDragDropTarget);
		SubscribeLocalEvent<JuggernautComponent, MobStateChangedEvent>(OnJuggernautStateChanged);
	}

	public void TryApplySoulStone(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
    {
		if (args.Target == null)
			return;

		// Check if target is a juggernaut shell
		if (HasComp<BloodCultConstructShellComponent>(args.Target))
		{
			_ActivateJuggernautShell(ent, args.User, args.Target.Value);
			args.Handled = true;
			return;
		}

		// Check if target is an inactive juggernaut
		if (TryComp<JuggernautComponent>(args.Target, out var juggComp) && juggComp.IsInactive)
		{
			_ReactivateJuggernaut(ent, args.User, args.Target.Value, juggComp);
			args.Handled = true;
			return;
		}
	}

	private void _ActivateJuggernautShell(EntityUid soulstone, EntityUid user, EntityUid shell)
	{
		// Get the mind from the soulstone
		EntityUid? mindId = CompOrNull<MindContainerComponent>(soulstone)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-soulstone-empty"), user, user, PopupType.Medium);
			return;
		}
		
		var shellCoordinates = Transform(shell).Coordinates;
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), shellCoordinates);
		
		// Delete the shell and spawn the juggernaut
		QueueDel(shell);
		var juggernaut = Spawn("MobBloodCultJuggernaut", shellCoordinates);
		
		// Store the soulstone in the juggernaut's container
		if (_container.TryGetContainer(juggernaut, "juggernaut_soulstone_container", out var soulstoneContainer))
		{
			_container.Insert(soulstone, soulstoneContainer);
		}
		
		// Store reference to soulstone in the juggernaut component and set as active
		if (TryComp<JuggernautComponent>(juggernaut, out var juggComp))
		{
			juggComp.SourceSoulstone = soulstone;
			juggComp.IsInactive = false;
		}
		
		// Transfer mind from soulstone to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, mind:mindComp);
		
		// Play transformation audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), shellCoordinates);
		
		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-created"), user, user, PopupType.Large);
	}

	private void _ReactivateJuggernaut(EntityUid soulstone, EntityUid user, EntityUid juggernaut, JuggernautComponent juggComp)
	{
		// Get the mind from the soulstone
		EntityUid? mindId = CompOrNull<MindContainerComponent>(soulstone)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-soulstone-empty"), user, user, PopupType.Medium);
			return;
		}

		// Store the soulstone in the juggernaut's container
		if (_container.TryGetContainer(juggernaut, "juggernaut_soulstone_container", out var soulstoneContainer))
		{
			_container.Insert(soulstone, soulstoneContainer);
		}

		// Store reference to soulstone and reactivate the juggernaut
		juggComp.SourceSoulstone = soulstone;
		juggComp.IsInactive = false;

		// DON'T heal the juggernaut - it stays in critical state until healed with blood

		// Transfer mind from soulstone to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, mind: mindComp);

		// Play transformation audio
		var coordinates = Transform(juggernaut).Coordinates;
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);

		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-reactivated"), user, user, PopupType.Large);
	}

	private void OnCanDropTarget(EntityUid uid, BloodCultConstructShellComponent component, ref CanDropTargetEvent args)
	{
		// Check if the dragged entity is a dead body with a mind
		args.CanDrop = _mobState.IsDead(args.Dragged) && 
		               CompOrNull<MindContainerComponent>(args.Dragged)?.Mind != null;
		args.Handled = true;
	}

	private void OnDragDropTarget(EntityUid uid, BloodCultConstructShellComponent component, ref DragDropTargetEvent args)
	{
		// Verify the dragged entity is a dead body with a mind
		if (!_mobState.IsDead(args.Dragged))
		{
			_popup.PopupEntity(Loc.GetString("cult-juggernaut-shell-needs-dead"), args.User, args.User, PopupType.Medium);
			return;
		}

		EntityUid? mindId = CompOrNull<MindContainerComponent>(args.Dragged)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-invocation-fail-nosoul"), args.User, args.User, PopupType.Medium);
			return;
		}

		var shellCoordinates = Transform(uid).Coordinates;
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), shellCoordinates);
		
		// Delete the shell and spawn the juggernaut
		QueueDel(uid);
		var juggernaut = Spawn("MobBloodCultJuggernaut", shellCoordinates);
		
		// Get the juggernaut's body container
		if (_container.TryGetContainer(juggernaut, "juggernaut_body_container", out var container))
		{
			// Insert the victim's body into the juggernaut
			_container.Insert(args.Dragged, container);
		}
		
		// Transfer mind from victim to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, mind:mindComp);
		
		// Play transformation audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), shellCoordinates);
		
		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-created"), args.User, args.User, PopupType.Large);
		
		args.Handled = true;
	}

	private void OnJuggernautStateChanged(Entity<JuggernautComponent> juggernaut, ref MobStateChangedEvent args)
	{
		// Only handle transition to critical state
		if (args.NewMobState != MobState.Critical)
			return;

		// Don't eject soulstone if already inactive
		if (juggernaut.Comp.IsInactive)
			return;

		// Check if the juggernaut has a soulstone
		if (juggernaut.Comp.SourceSoulstone == null)
			return;

		var soulstone = juggernaut.Comp.SourceSoulstone.Value;

		// Verify the soulstone still exists
		if (!Exists(soulstone))
			return;

		// Get the juggernaut's mind
		EntityUid? mindId = CompOrNull<MindContainerComponent>(juggernaut)?.Mind;
		if (mindId == null || !TryComp<MindComponent>(mindId, out var mindComp))
			return;

		// Transfer the mind back to the soulstone
		_mind.TransferTo((EntityUid)mindId, soulstone, mind: mindComp);

		// Remove the soulstone from the container and spawn it at the juggernaut's location
		if (_container.TryGetContainer(juggernaut, "juggernaut_soulstone_container", out var container))
		{
			_container.Remove(soulstone, container);
		}

		// Play audio effect
		var coordinates = Transform(juggernaut).Coordinates;
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);

		// Show popup
		_popup.PopupEntity(
			Loc.GetString("cult-juggernaut-critical-soulstone-ejected"),
			juggernaut, PopupType.LargeCaution
		);

		// Mark the juggernaut as inactive and clear the reference
		juggernaut.Comp.IsInactive = true;
		juggernaut.Comp.SourceSoulstone = null;
	}
}
