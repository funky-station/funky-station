// SPDX-FileCopyrightText: 2023 dahnte <70238020+dahnte@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 W.xyz() <84605679+pirakaplant@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class FrontalLispSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    // @formatter:off
    private static readonly Regex RegexUpperTh = new(@"T+S+(?=[\p{Lu}\W])|S+C+(?=[IEY]+)|C+(?=[IEY]+)|PS+(?=[\p{Lu}\W])|(S+T+|T+)(?=I+O+U*N*)|C+H+(?=I*E*)|Z+(?=[\p{Lu}\W])|S+(?=[\p{Lu}\W])|X+(?=E+)");
    private static readonly Regex RegexLowerTh = new(@"t+s+|s+c+(?=[iey]+)|c+(?=[iey]+)|ps+|(s+t+|t+)(?=i+o+u*n*)|c+h+(?=i*e*)|z+|s+|x+(?=e+)");
    private static readonly Regex RegexSentenceTh = new(@"T+s+|S+c+(?=[iey]+)|C+(?=[iey]+)|Ps+|(S+T+|T+)(?=i+o+u*n*)|C+h+(?=i*e*)|Z+|S+|X+(?=e+)"); // Funky
    private static readonly Regex RegexUpperEcks = new(@"E+[Xx]+[Cc]*|(?<![AaIiOoUu])X+(?=-*\p{Lu})"); // Funky - For edge cases like "X-ray"
    private static readonly Regex RegexLowerEcks = new(@"e+x+c*|(?<![AaIiOoUu])x+");
    private static readonly Regex RegexSentenceEcks = new(@"Ee*x+c*]*|(?<![AaIiOoUu])X+(?!\p{Lu})"); // Funky
    private static readonly Regex RegexUpperEcksAfterVowel = new(@"(?=[AaIiOoUu])X+(?=\p{Lu})"); // Funky
    private static readonly Regex RegexLowerEcksAfterVowel = new(@"(?=[AaIiOoUu])x+"); // Funky
    // @formatter:on

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent, after: new[] {typeof(BostonAccentSystem)}); // Funky - Fixes a weird conflict with the Boston accent
    }

    private void OnAccent(EntityUid uid, FrontalLispComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "lisp"); // Funky - This ReplacementAccent is under the _Funkystation namespace since it originates from here.

        // handles ts, sc(i|e|y), c(i|e|y), ps, st(io(u|n)), ch(i|e), z, s
        message = RegexUpperTh.Replace(message, "TH");
        message = RegexLowerTh.Replace(message, "th");
        message = RegexSentenceTh.Replace(message, "Th");
        // handles ex(c), x
        message = RegexUpperEcks.Replace(message, "EKTH");
        message = RegexLowerEcks.Replace(message, "ekth");
        message = RegexSentenceEcks.Replace(message, "Ekth");
        message = RegexUpperEcksAfterVowel.Replace(message, "KTH");
        message = RegexLowerEcksAfterVowel.Replace(message, "kth");

        args.Message = message;
    }
}
