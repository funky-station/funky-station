// SPDX-FileCopyrightText: 2024 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 W.xyz() <tptechteam@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

// Copying the GermanAccentSystem REUSE header since this is just code from there split from the original file

using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

public sealed class GermanVernacularSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexThe = new(@"(?<=\s|^)the(?=\s|$)", RegexOptions.IgnoreCase);
    private static readonly Regex RegexAUmlautUpper = new(@"Ä");
    private static readonly Regex RegexAUmlautLower = new(@"ä");
    private static readonly Regex RegexOUmlautUpper = new(@"Ö");
    private static readonly Regex RegexOUmlautLower = new(@"ö");
    private static readonly Regex RegexUUmlautUpper = new(@"Ü");
    private static readonly Regex RegexUUmlautLower = new(@"ü");

    public override void Initialize()
    {
        SubscribeLocalEvent<GermanVernacularComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        var msg = message;

        // The "the" -> "das" system below this requires a three-letter word.
        // So we undo the "ze" -> "the" system! This sucks so much.
        msg = _replacement.ApplyReplacements(msg, "germanthe");

        foreach (Match match in RegexThe.Matches(msg))
        {
            if (_random.Prob(0.3f))
            {
                // just shift T, H and E over to D, A and S to preserve capitalization
                msg = msg.Substring(0, match.Index) +
                      (char)(msg[match.Index] - 16) +
                      (char)(msg[match.Index + 1] - 7) +
                      (char)(msg[match.Index + 2] + 14) +
                      msg.Substring(match.Index + 3);
            }
        }

        // Also we're undoing all the umlauts because they mess with word replacement.
        msg = RegexAUmlautUpper.Replace(message, "A");
        msg = RegexAUmlautLower.Replace(message, "a");
        msg = RegexOUmlautUpper.Replace(message, "O");
        msg = RegexOUmlautLower.Replace(message, "o");
        msg = RegexUUmlautUpper.Replace(message, "U");
        msg = RegexUUmlautLower.Replace(message, "u");

        msg = _replacement.ApplyReplacements(msg, "german");

        // Time to put them back!
        var msgBuilder = new StringBuilder(msg);

        var umlautCooldown = 0;
        for (var i = 0; i < msgBuilder.Length; i++)
        {
            if (umlautCooldown == 0)
            {
                if (_random.Prob(0.1f)) // 10% of all eligible vowels become umlauts)
                {
                    msgBuilder[i] = msgBuilder[i] switch
                    {
                        'A' => 'Ä',
                        'a' => 'ä',
                        'O' => 'Ö',
                        'o' => 'ö',
                        'U' => 'Ü',
                        'u' => 'ü',
                        _ => msgBuilder[i]
                    };
                    umlautCooldown = 4;
                }
            }
            else
            {
                umlautCooldown--;
            }
        }

        // Now everything should be back to normal!
        return msg;
    }

    private void OnAccent(Entity<GermanVernacularComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
