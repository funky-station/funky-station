using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class WordleUi : UIFragment
{
    private WordleUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new WordleUiFragment();
        _fragment.OnLetterPressed += letter => SendWordleMessage(WordleUiAction.GuessLetter, letter, userInterface);
        _fragment.OnBackspacePressed += () => SendWordleMessage(WordleUiAction.Backspace, '\0', userInterface);
        _fragment.OnSubmitPressed += () => SendWordleMessage(WordleUiAction.SubmitGuess, '\0', userInterface);
        _fragment.OnNewGamePressed += () => SendWordleMessage(WordleUiAction.NewGame, '\0', userInterface);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not WordleUiState wordleState)
            return;

        _fragment?.UpdateState(wordleState);
    }

    private void SendWordleMessage(WordleUiAction action, char letter, BoundUserInterface userInterface)
    {
        var wordleMessage = new WordleUiMessageEvent(action, letter);
        var message = new CartridgeUiMessage(wordleMessage);
        userInterface.SendMessage(message);
    }
}
