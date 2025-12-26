// SPDX-FileCopyrightText: 2025 W.xyz() <tptechteam@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Speech.Components;
using System.Text.RegularExpressions;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems;

public sealed class CakAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexWhitespace = new(@"\s");

    public override void Initialize()
    {
        SubscribeLocalEvent<CakAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        // This is atrocious but makes sure that the word is not part of a longer sentence.
        if (RegexWhitespace.Count(message) == 2)
        {
            message = _replacement.ApplyReplacements(message, "cak2spaces");
        }
        else if (RegexWhitespace.Count(message) == 1)
        {
            message = _replacement.ApplyReplacements(message, "cak1space");
        }
        else if (RegexWhitespace.Count(message) == 0)
        {
            message = _replacement.ApplyReplacements(message, "cak0spaces");
        }
        return message;
    }

    private void OnAccent(Entity<CakAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
