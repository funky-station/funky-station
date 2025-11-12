// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class BarkAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly IReadOnlyList<string> Barks = new List<string>{
            " Woof!", " WOOF", " wof-wof"
        }.AsReadOnly();

        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            { "ah", "arf" },
            { "Ah", "Arf" },
        };

        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'a', 'e', 'i', 'o', 'u',
            'A', 'E', 'I', 'O', 'U'
        };

        private static readonly HashSet<string> ShortWordExceptions = new HashSet<string>
        {
            "uh", "Uh", "UH",
            "oh", "Oh", "OH"
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<BarkAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            foreach (var (word, repl) in SpecialWords)
            {
                message = message.Replace(word, repl);
            }

            // Add 'r' to words starting with vowels
            var words = message.Split(' ');
            var result = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0)
                    result.Append(' ');

                var word = words[i];
                if (word.Length > 0 && Vowels.Contains(word[0]))
                {
                    // Skip 1-3 letter words unless they're exceptions (uh, oh)
                    if (word.Length <= 3 && !ShortWordExceptions.Contains(word))
                    {
                        result.Append(word);
                        continue;
                    }

                    // Add 'R' for uppercase vowels, 'r' for lowercase vowels
                    if (char.IsUpper(word[0]))
                    {
                        result.Append('R');
                        // Check if word is fully capitalized
                        bool isFullyCaps = word.Length > 1 && word.All(c => !char.IsLetter(c) || char.IsUpper(c));

                        if (isFullyCaps)
                        {
                            // Keep the vowel capitalized for fully caps words
                            result.Append(word);
                        }
                        else
                        {
                            // Make the first letter lowercase after adding capital R
                            result.Append(char.ToLower(word[0]));
                            if (word.Length > 1)
                                result.Append(word.Substring(1));
                        }
                    }
                    else
                    {
                        result.Append('r');
                        result.Append(word);
                    }
                }
                else if (word.Length > 0 && char.IsLetter(word[0]))
                {
                    // Replace consonant with 'R' or 'r'
                    if (char.IsUpper(word[0]))
                    {
                        result.Append('R');
                        if (word.Length > 1)
                            result.Append(word.Substring(1));
                    }
                    else
                    {
                        result.Append('r');
                        if (word.Length > 1)
                            result.Append(word.Substring(1));
                    }
                }
                else
                {
                    result.Append(word);
                }
            }

            return result.ToString();
        }

        private void OnAccent(EntityUid uid, BarkAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
