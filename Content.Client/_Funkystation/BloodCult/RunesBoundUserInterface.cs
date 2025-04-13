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