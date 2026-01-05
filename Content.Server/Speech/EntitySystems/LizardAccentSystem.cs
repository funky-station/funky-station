// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerS = new("s+(?![h|H])");
    private static readonly Regex RegexUpperS = new("S+(?![h|H])");
    private static readonly Regex RegexInternalLowerX = new(@"(\w)x");
    private static readonly Regex RegexInternalUpperX = new(@"(\w)X");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // eshit
        message = RegexInternalLowerX.Replace(message, "$1sh");
        // ESHIT
        message = RegexInternalUpperX.Replace(message, "$1SH");
        // esh-ray
        message = RegexLowerEndX.Replace(message, "esh$1");
        // ESH-ray
        message = RegexUpperEndX.Replace(message, "ESH$1");

        args.Message = message;
    }
}
