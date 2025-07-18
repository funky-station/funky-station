// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.Prototypes;
using Content.Client._Funkystation.BloodCult.UI;

namespace Content.Client._Funkystation.BloodCult;

public sealed class SpellsBoundUserInterface : BoundUserInterface
{
	[Dependency] private readonly IClyde _displayManager = default!;
	[Dependency] private readonly IInputManager _inputManager = default!;

	private SpellRadialMenu? _spellRitualMenu;

	public SpellsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
	{
		IoCManager.InjectDependencies(this);
	}

	protected override void Open()
	{
		base.Open();

		_spellRitualMenu = this.CreateWindow<SpellRadialMenu>();
		_spellRitualMenu.SetEntity(Owner);
		_spellRitualMenu.SendSpellsMessageAction += SendSpellsMessage;

		var vpSize = _displayManager.ScreenSize;
		_spellRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
	}

	private void SendSpellsMessage(ProtoId<CultAbilityPrototype> protoId)
	{
		SendMessage(new SpellsMessage(protoId));
	}
}