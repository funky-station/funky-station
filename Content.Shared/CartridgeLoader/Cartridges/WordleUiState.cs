using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class WordleUiState : BoundUserInterfaceState
{
    public string CurrentGuess;
    public List<string> PreviousGuesses;
    public List<List<int>> LetterStates; // 0 = not guessed, 1 = wrong spot, 2 = correct spot, 3 = not in word
    public int AttemptsRemaining;
    public bool GameWon;
    public bool GameLost;
    public string? SecretWord;
    public bool InvalidWordError;

    public WordleUiState(string currentGuess, List<string> previousGuesses, List<List<int>> letterStates,
        int attemptsRemaining, bool gameWon, bool gameLost, string? secretWord = null, bool invalidWordError = false)
    {
        CurrentGuess = currentGuess;
        PreviousGuesses = previousGuesses;
        LetterStates = letterStates;
        AttemptsRemaining = attemptsRemaining;
        GameWon = gameWon;
        GameLost = gameLost;
        SecretWord = secretWord;
        InvalidWordError = invalidWordError;
    }
}
