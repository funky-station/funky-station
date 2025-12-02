using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class WordleUiMessageEvent : CartridgeMessageEvent
{
    public readonly WordleUiAction Action;
    public readonly char Letter;

    public WordleUiMessageEvent(WordleUiAction action, char letter = '\0')
    {
        Action = action;
        Letter = letter;
    }
}

[Serializable, NetSerializable]
public enum WordleUiAction
{
    GuessLetter,
    SubmitGuess,
    NewGame,
    Backspace
}
