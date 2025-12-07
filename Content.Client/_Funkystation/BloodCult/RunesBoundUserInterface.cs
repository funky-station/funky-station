// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._Funkystation.BloodCult.UI;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Content.Shared.BloodCult.Components;

namespace Content.Client._Funkystation.BloodCult;

public sealed class RunesBoundUserInterface : BoundUserInterface
{
	[Dependency] private readonly IClyde _displayManager = default!;
	[Dependency] private readonly IInputManager _inputManager = default!;

	private RuneRadialMenu? _runeRitualMenu;

	public RunesBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
	{
		IoCManager.InjectDependencies(this);
	}

	protected override void Open()
	{
		base.Open();

		_runeRitualMenu = this.CreateWindow<RuneRadialMenu>();
		_runeRitualMenu.SetEntity(Owner);
		_runeRitualMenu.SendRunesMessageAction += SendRunesMessage;//SendHereticRitualMessage;

		var vpSize = _displayManager.ScreenSize;
		_runeRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
	}

	private void SendRunesMessage(string protoId)
	{
		SendMessage(new RunesMessage(protoId));
	}
}