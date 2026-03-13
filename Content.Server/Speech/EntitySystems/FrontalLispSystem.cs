// SPDX-FileCopyrightText: 2023 dahnte <70238020+dahnte@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class FrontalLispSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    // @formatter:off
    private static readonly Regex RegexUpperTh = new(@"T+S+|S+C+(?=[IEY]+)|C+(?=[IEY]+)|PS+|(S+T+|T+)(?=I+O+U*N*)|C+H+(?=I*E*)|Z+|S+X+(?=E+)");
    private static readonly Regex RegexLowerTh = new(@"t+s+|s+c+(?=[iey]+)|c+(?=[iey]+)|ps+|(s+t+|t+)(?=i+o+u*n*)|c+h+(?=i*e*)|z+|s+|x+(?=e+)");
     private static readonly Regex RegexSentenceTh = new(@"T+[Ss]+|S+[Cc]+(?=[iey]+)|C+(?=[iey]+)|P[Ss]+|(S+[Tt]+|T+)(?=i+o+u*n*)|C+h+(?=i*e*)|Z+|S+|X+(?=e+)"); // Funky
    private static readonly Regex RegexUpperEcks = new(@"E+[Xx]+Cc*|X+");
    private static readonly Regex RegexLowerEcks = new(@"e+x+c*|x+");
    // @formatter:on

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent);
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

        args.Message = message;
    }
}
