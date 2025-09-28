// Uses some code from SouthernAccentSystem.cs, credited to GitHub users Pieter-Jan Briers and UBlueberry under AGPL-3.0-or-later.
// Also uses some code from Goob Station's BoganAccentSystem.cs, credited to GitHub users Aidenkrz, BeeRobyn, Piras314, Misandry, and gus under AGPL-3.0-or-later.

using Robust.Shared.Random;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class BoganAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerIng = new(@"ing\b");
    private static readonly Regex RegexUpperIng = new(@"ING\b");
    private static readonly Regex RegexLowerDve = new(@"d've\b");
    private static readonly Regex RegexUpperDve = new(@"D'VE\b");

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BoganAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, BoganAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "bogan");
        message = RegexLowerIng.Replace(message, "in'");
        message = RegexUpperIng.Replace(message, "IN'");
        message = RegexLowerDve.Replace(message, "da");
        message = RegexUpperDve.Replace(message, "DA");

        // Prefix
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 4);

            // Reverse sanitize capital
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-bogan-prefix-{pick}") + " " + message;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffixes
        // Also sorry for the shitcode. Hopefully someone can make this better.
        if (_random.Prob(0.3f))
        {
            if (message.EndsWith('.'))
            {
                message = message.Remove(message.Length - 1);
            }
            var pick = _random.Next(1, 5);
            if (message.EndsWith('!'))
            {
                message += Loc.GetString($"accent-bogan-suffix-{pick}") + "!";
            }
            else if (message.EndsWith('?'))
            {
                message = message.Remove(message.Length - 1);
                message += Loc.GetString($"accent-bogan-suffix-{pick}") + "?";
            }
            else if (message.EndsWith('‽'))
            {
                message = message.Remove(message.Length - 1);
                message += Loc.GetString($"accent-bogan-suffix-{pick}") + "‽";
            }
            else
            {
                message += Loc.GetString($"accent-bogan-suffix-{pick}") + ".";
            }

        }
        args.Message = message;
    }
};
