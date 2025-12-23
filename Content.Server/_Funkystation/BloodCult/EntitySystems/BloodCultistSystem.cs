// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Content.Shared.BloodCult;

namespace Content.Server.BloodCult;

public sealed class BloodCultistSystem : SharedBloodCultistSystem
{
	[Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

	public override void Initialize()
	{
		base.Initialize();
		SubscribeLocalEvent<BloodCultCommuneEvent>(OpenCommuneUI);
		SubscribeLocalEvent<BloodCultSpellsEvent>(OpenSpellsUI);
	}

	#region CommuneUI
	private void OpenCommuneUI(BloodCultCommuneEvent ev)
	{
		// Use the performer (cultist) directly - it's already an EntityUid
		var communeEntity = ev.Performer;

		Logger.Debug($"OpenCommuneUI called for entity {communeEntity}");

		// Early return if already handled
		if (ev.Handled)
		{
			Logger.Debug($"OpenCommuneUI: Event already handled");
			return;
		}

		if (!TryComp<BloodCultistComponent>(communeEntity, out var cultistComp))
		{
			Logger.Warning($"OpenCommuneUI: Entity {communeEntity} does not have BloodCultistComponent");
			return;
		}

		if (!_uiSystem.HasUi(communeEntity, BloodCultistCommuneUIKey.Key))
		{
			Logger.Warning($"OpenCommuneUI: Entity {communeEntity} does not have BloodCultistCommuneUIKey UI");
			return;
		}

		if (_uiSystem.IsUiOpen(communeEntity, BloodCultistCommuneUIKey.Key))
		{
			Logger.Debug($"OpenCommuneUI: UI already open");
			ev.Handled = true;
			return;
		}

		Logger.Debug($"OpenCommuneUI: Attempting to open UI");
		if (_uiSystem.TryOpenUi(communeEntity, BloodCultistCommuneUIKey.Key, communeEntity))
		{
			Logger.Debug($"OpenCommuneUI: UI opened successfully");
			UpdateCommuneUI((communeEntity, cultistComp));
		}
		else
		{
			Logger.Warning($"OpenCommuneUI: Failed to open UI");
		}

		ev.Handled = true;
	}

	private void UpdateCommuneUI(Entity<BloodCultistComponent> entity)
	{
		if (_uiSystem.HasUi(entity, BloodCultistCommuneUIKey.Key))
			_uiSystem.SetUiState(entity.Owner, BloodCultistCommuneUIKey.Key, new BloodCultCommuneBuiState(""));
	}
	#endregion

	#region SpellsUI
	private void OpenSpellsUI(BloodCultSpellsEvent ev)
	{
		// Use the performer (cultist) directly - it's already an EntityUid
		var spellsEntity = ev.Performer;

		Logger.Debug($"OpenSpellsUI called for entity {spellsEntity}");

		// Early return if already handled
		if (ev.Handled)
		{
			Logger.Debug($"OpenSpellsUI: Event already handled");
			return;
		}

		if (!TryComp<BloodCultistComponent>(spellsEntity, out var cultistComp))
		{
			Logger.Warning($"OpenSpellsUI: Entity {spellsEntity} does not have BloodCultistComponent");
			return;
		}

		if (!_uiSystem.HasUi(spellsEntity, SpellsUiKey.Key))
		{
			Logger.Warning($"OpenSpellsUI: Entity {spellsEntity} does not have SpellsUiKey UI");
			return;
		}

		if (_uiSystem.IsUiOpen(spellsEntity, SpellsUiKey.Key))
		{
			Logger.Debug($"OpenSpellsUI: UI already open");
			ev.Handled = true;
			return;
		}

		Logger.Debug($"OpenSpellsUI: Attempting to open UI");
		if (_uiSystem.TryOpenUi(spellsEntity, SpellsUiKey.Key, spellsEntity))
		{
			Logger.Debug($"OpenSpellsUI: UI opened successfully");
			UpdateSpellsUI((spellsEntity, cultistComp));
		}
		else
		{
			Logger.Warning($"OpenSpellsUI: Failed to open UI");
		}

		ev.Handled = true;
	}

	private void UpdateSpellsUI(Entity<BloodCultistComponent> entity)
	{
		if (_uiSystem.HasUi(entity, SpellsUiKey.Key))
			_uiSystem.SetUiState(entity.Owner, SpellsUiKey.Key, new BloodCultSpellsBuiState());
	}
	#endregion

	#region RuneEvents
	public void UseReviveRune(EntityUid target, EntityUid? user, EntityUid? used)
	{
		var attempt = new ReviveRuneAttemptEvent(target, user, used);
		RaiseLocalEvent(target, attempt, true);
	}

	public void UseGhostifyRune(EntityUid target, EntityUid? user, EntityUid used)
	{
		var attempt = new GhostifyRuneEvent(target, user, used);
		RaiseLocalEvent(target, attempt, true);
	}

	public void UseSacrificeRune(EntityUid target, EntityUid user, EntityUid used, EntityUid[] otherCultists)
	{
		var attempt = new SacrificeRuneEvent(target, user, used, otherCultists);
		RaiseLocalEvent(user, attempt, true);
	}

	public void UseConvertRune(EntityUid target, EntityUid user, EntityUid used, EntityUid[] otherCultists)
	{
		var attempt = new ConvertRuneEvent(target, user, used, otherCultists);
		RaiseLocalEvent(user, attempt, true);
	}
	#endregion
}
