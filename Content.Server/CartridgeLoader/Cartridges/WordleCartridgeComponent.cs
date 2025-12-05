// SPDX-FileCopyrightText: 2025 rex1431ify <r.l@live.se>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.IO;

namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class WordleCartridgeComponent : Component
{
    public const string ProgramName = "wordle-program-name";
    /// <summary>
    /// The secret word to guess (5 letters)
    /// </summary>
    [DataField]
    public string SecretWord = "";

    /// <summary>
    /// The current guess being typed (0-5 letters)
    /// </summary>
    public string CurrentGuess = "";

    /// <summary>
    /// List of all previous guesses made (each is 5 letters)
    /// </summary>
    public List<string> PreviousGuesses = new();

    /// <summary>
    /// Letter states for each guess: 1 = wrong position, 2 = correct position, 3 = not in word
    /// Each entry is a list of 5 states corresponding to the 5 letters
    /// </summary>
    public List<List<int>> LetterStates = new();

    /// <summary>
    /// Number of attempts remaining (starts at 6)
    /// </summary>
    public int AttemptsRemaining = 6;

    /// <summary>
    /// Whether the current game has been won
    /// </summary>
    public bool GameWon = false;

    /// <summary>
    /// Whether the current game has been lost
    /// </summary>
    public bool GameLost = false;

    /// <summary>
    /// Valid words list - all possible 5-letter words that can be guessed or chosen as secret word
    /// Loaded from all_wordle_words.txt file
    /// </summary>
    public static readonly List<string> ValidWords = LoadValidWords();

    private static List<string> LoadValidWords()
    {
        var words = new List<string>();
        try
        {
            // Try multiple possible locations for the word list file
            var possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "all_wordle_words.txt"),
                Path.Combine(AppContext.BaseDirectory, "Resources", "all_wordle_words.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "all_wordle_words.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", "all_wordle_words.txt"),
                "all_wordle_words.txt" // Current directory
            };

            string? foundPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    foundPath = path;
                    break;
                }
            }

            if (foundPath != null)
            {
                var lines = File.ReadAllLines(foundPath);
                words = lines
                    .Select(line => line.Trim().ToUpper())
                    .Where(line => !string.IsNullOrEmpty(line) && line.Length == 5)
                    .ToList();
            }
        }
        catch (Exception)
        {
            // Silently fail to load word list
        }
        return words;
    }

    public static string GetRandomWord()
    {
        if (ValidWords.Count == 0)
        {
            return "ABOUT"; // Fallback word if no words loaded
        }
        var random = new Random();
        var word = ValidWords[random.Next(ValidWords.Count)];
        return word;
    }

    /// <summary>
    /// Checks if a word is valid (in the ValidWords list)
    /// </summary>
    public static bool IsValidWord(string word)
    {
        var upper = word.ToUpper();
        var isValid = ValidWords.Contains(upper);
        return isValid;
    }
}
