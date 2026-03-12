// SPDX-FileCopyrightText: 2026 W.xyz() <84605679+pirakaplant@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 W.xyz() <tptechteam@gmail.com>
// SPDX-FileCopyrightText: 2026 w.xyz() <84605679+pirakaplant@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

public sealed class BostonAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowercaseAr = new(@"\Bar(?=(s\b|\b))(?!re)");
    private static readonly Regex RegexUppercaseAr = new(@"\BAR(?=(s\b|\b))(?!re)");
    private static readonly Regex RegexLowercaseEr = new(@"er\B");
    private static readonly Regex RegexUppercaseEr = new(@"ER\B");
    private static readonly Regex RegexSentenceCaseEr = new(@"Er\B");
    private static readonly Regex RegexLowercaseEndingEr = new(@"\Ber(?=(s\b|\b))");
    private static readonly Regex RegexUppercaseEndingEr = new(@"\BER(?=(s\b|\b))");
    private static readonly Regex RegexLowercaseEndingOr = new(@"\Bor(?=(s\b|\b))");
    private static readonly Regex RegexUppercaseEndingOr = new(@"\BOR(?=(s\b|\b))");
    private static readonly Regex RegexLowercaseNty = new(@"\Bnt(?=(y|ie))");
    private static readonly Regex RegexUppercaseNty = new(@"\BNT(?=(Y|IE))");
    private static readonly Regex RegexApostropheT = new(@"'t", RegexOptions.IgnoreCase);

    public override void Initialize()
    {
        SubscribeLocalEvent<BostonAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        var msg = message;

        // start -> staht
        // this doesn't change "are" because that feels wrong to me
        msg = RegexLowercaseAr.Replace(msg, "ah");
        msg = RegexUppercaseAr.Replace(msg, "AH");

        // error -> ehror
        msg = RegexLowercaseEr.Replace(msg, "eh");
        msg = RegexUppercaseEr.Replace(msg, "EH");
        msg = RegexSentenceCaseEr.Replace(msg, "Eh");

        // meter -> metah
        msg = RegexLowercaseEndingEr.Replace(msg, "ah");
        msg = RegexUppercaseEndingEr.Replace(msg, "AH");

        // reactor -> reactah
        msg = RegexLowercaseEndingOr.Replace(msg, "ah");
        msg = RegexUppercaseEndingOr.Replace(msg, "AH");

        // bounty -> bounny
        msg = RegexLowercaseNty.Replace(msg, "nn");
        msg = RegexUppercaseNty.Replace(msg, "NN");

        // don't -> don'
        msg = RegexApostropheT.Replace(msg, "'");

        return msg;
    }
    private void OnAccent(Entity<BostonAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
