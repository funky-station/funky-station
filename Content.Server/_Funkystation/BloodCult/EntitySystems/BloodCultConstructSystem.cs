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
using Content.Shared.Popups;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

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
	}

	public void TryApplySoulStone(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
    {
		if (args.Target == null || !HasComp<BloodCultConstructShellComponent>(args.Target))
			return;
		var coordinates = Transform((EntityUid)args.Target).Coordinates;
		EntityUid? mindId = CompOrNull<MindContainerComponent>(ent)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		if (mindId != null && mindComp != null)
		{
			QueueDel((EntityUid)args.Target);
			var construct = Spawn("MobBloodCultJuggernaut", coordinates);
			_mind.TransferTo((EntityUid)mindId, construct, mind:mindComp);
			QueueDel(ent);
		}
		args.Handled = true;
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
}
