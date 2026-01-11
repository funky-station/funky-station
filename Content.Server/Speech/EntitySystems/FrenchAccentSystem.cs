// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 TakoDragon <69509841+BackeTako@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 W.xyz() <tptechteam@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 w.xyz() <84605679+pirakaplant@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// System that gives the speaker a faux-French accent.
/// </summary>
public sealed class FrenchAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexThLower = new(@"th");
    private static readonly Regex RegexThUpper = new(@"T(?i)h");
    private static readonly Regex RegexStartH = new(@"(?<!\w)h(?!m)(?!uh\W)", RegexOptions.IgnoreCase);
    private static readonly Regex RegexSpacePunctuation = new(@"(?<=\w\w)[!?;:](?!\w)", RegexOptions.IgnoreCase);
    private static readonly Regex RegexEndErLower = new(@"(?<=\w)(?<!e)er(?!\w)");
    private static readonly Regex RegexEndErsLower = new(@"(?<=\w)(?<!e)ers(?!\w)");
    private static readonly Regex RegexEndErUpper = new(@"(?<=\w)(?<!E)ER(?!\w)");
    private static readonly Regex RegexEndErsUpper = new(@"(?<=\w)(?<!E)ERS(?!\w)");
    private static readonly Regex RegexEndOrLower = new(@"(?<=\w)(?<!o)or(?!\w)");
    private static readonly Regex RegexEndOrsLower = new(@"(?<=\w)(?<!o)ors(?!\w)");
    private static readonly Regex RegexEndOrUpper = new(@"(?<=\w)(?<!O)OR(?!\w)");
    private static readonly Regex RegexEndOrsUpper = new(@"(?<=\w)(?<!O)ORS(?!\w)");
    private static readonly Regex RegexEndVLower = new(@"(?<=\w)v(?!\w)");
    private static readonly Regex RegexEndVsLower = new(@"(?<=\w)vs(?!\w)");
    private static readonly Regex RegexEndVUpper = new(@"(?<=\w)V(?!\w)");
    private static readonly Regex RegexEndVsUpper = new(@"(?<=\w)VS(?!\w)");
    private static readonly Regex RegexEndVeLower = new(@"(?<=\w)ve(?!\w)");
    private static readonly Regex RegexEndVesLower = new(@"(?<=\w)ves(?!\w)");
    private static readonly Regex RegexEndVeUpper = new(@"(?<=\w)VE(?!\w)");
    private static readonly Regex RegexEndVesUpper = new(@"(?<=\w)VES(?!\w)");
    private static readonly Regex RegexEndIcLower = new(@"(?<=\w)ic(?!\w)");
    private static readonly Regex RegexEndIcsLower = new(@"(?<=\w)ics(?!\w)");
    private static readonly Regex RegexEndIcUpper = new(@"(?<=\w)IC(?!\w)");
    private static readonly Regex RegexEndIcsUpper = new(@"(?<=\w)ICS(?!\w)");
    private static readonly Regex RegexOpeningDoubleQuote = new(@"(?<!\w)""(?=\w)");
    private static readonly Regex RegexClosingDoubleQuote = new(@"(?<=\w)""(?!\w)");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrenchAccentComponent, AccentGetEvent>(OnAccentGet, after: new[] {typeof(ReplacementAccentSystem)});
    }

    public string Accentuate(string message, FrenchAccentComponent component)
    {
        var msg = message;

        // replaces th with z
        msg = RegexThLower.Replace(msg, "'z");
        msg = RegexThUpper.Replace(msg, "'Z");

        // replaces h with ' at the start of words, unless they're going "hm" or "huh".
        msg = RegexStartH.Replace(msg, "'");

        // spaces out ! ? : and ;.
        msg = RegexSpacePunctuation.Replace(msg, " $&");

        // changes er(s) and or(s) to eur(s) at the end of words.
        msg = RegexEndErLower.Replace(msg, "eur");
        msg = RegexEndErsLower.Replace(msg, "eurs");
        msg = RegexEndErUpper.Replace(msg, "EUR");
        msg = RegexEndErsUpper.Replace(msg, "EURS");
        msg = RegexEndOrLower.Replace(msg, "eur");
        msg = RegexEndOrsLower.Replace(msg, "eurs");
        msg = RegexEndOrUpper.Replace(msg, "EUR");
        msg = RegexEndOrsUpper.Replace(msg, "EURS");

        // changes v(s) & ve(s) to f(s) at the end of words.
        msg = RegexEndVLower.Replace(msg, "f");
        msg = RegexEndVsLower.Replace(msg, "fs");
        msg = RegexEndVUpper.Replace(msg, "F");
        msg = RegexEndVsUpper.Replace(msg, "FS");
        msg = RegexEndVeLower.Replace(msg, "f");
        msg = RegexEndVesLower.Replace(msg, "fs");
        msg = RegexEndVeUpper.Replace(msg, "F");
        msg = RegexEndVesUpper.Replace(msg, "FS");

        // changes ic(s) to ique(s) at the end of words.
        msg = RegexEndIcLower.Replace(msg, "ique");
        msg = RegexEndIcsLower.Replace(msg, "iques");
        msg = RegexEndIcUpper.Replace(msg, "IQUE");
        msg = RegexEndIcsUpper.Replace(msg, "IQUES");

        // replace "quotation marks" with « spaced guillemets »
        msg = RegexOpeningDoubleQuote.Replace(msg, "« ");
        msg = RegexClosingDoubleQuote.Replace(msg, " »");

        return msg;
    }

    private void OnAccentGet(EntityUid uid, FrenchAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
