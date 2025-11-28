// SPDX-FileCopyrightText: 2025 phmnsx <lynnwastinghertime@gmail.com>
//
// SPDX-License-Identifier: MIT


using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Text.RegularExpressions;
using System.Linq;

namespace Content.Server.Speech.EntitySystems;

public sealed class BritishAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    // r/s(t)h/r exceptions for T, protected words will have a "|" added temporarily to avoid being affected
    private static readonly Regex RegexT = new(@"(?i)(?<=[^rsc\W])(t+)(?=[^rsh\W])(?!\S+\|)");
    //when a word that ends with t or d and is followed by a word, add a glottal stop
    private static readonly Regex RegexEndT = new(@"(?i)(d|t)\s(?=\w)");
    // Fix H in beginning of string
    private static readonly Regex RegexNakedH = new(@"(?i)(?<=^|\s)h(?=\w+)");
    // kill all the | 
    private static readonly Regex ClearBlockedWords = new(@"\|");
    private readonly IReadOnlyList<string> _endings = new List<string>(){ ", innit?", ", mate", ", bruv"};
    private readonly IReadOnlyList<string> _starts = new List<string>(){ "Oi bruv ", "Oi ", "Mate ", "Oi mate "};
    // startwords and endowrds are words used to check for duplicates, they're keywords from the other lists.
    // (Also makes it easier to add stuff!)
    private readonly List<string> _startwords = new(){"oi", "mate", "bruv"};
    private readonly List<string> _endwords = new(){"innit", "mate", "bruv"};

    //words that shouldnt be affected by regex
    private readonly List<string> _blockedWords = new(){"that", "together", "tomato", "mate", "dont",
                                                        "cant", "shouldnt", "wouldnt", "couldnt", "mightnt",
                                                        "mustnt", "wont", "isnt", "arent", "werent", "hadnt",
                                                        "doesnt", "didnt", "shant", "\'t", "after", "not"};

    public override void Initialize()
    {
        //Note: SubscribeLocalEvent has this signature where you can make some accents go before others,
        // for some reason i didn't see it before, so feel free to do that for any accent you create
        //made to prevent accents break some british "features", such as when a lizard speaks
        // "tuesday" -> "tuesssday" and it wouldn't become chewsday
        SubscribeLocalEvent<BritishAccentComponent, AccentGetEvent>(OnAccent, before: new[] { typeof(LizardAccentSystem), typeof(StutteringSystem), 
                                                                                              typeof(ScandinavianAccentSystem), typeof(RussianAccentSystem)}, 
                                                                              after: new [] { typeof(NoContractionsAccentComponentAccentSystem) });

    }

    public string Accentuate(string message)
    {
        var msg = message;
        msg.Trim();
        msg = _replacement.ApplyReplacements(msg, "british");

        //checking if the start or end should be uppercase, through "probability"
        //that is, if the first n letters are uppercase, the startshould likely be uppercase
        //as well.
        var msgNoSpace = msg.Replace(" ", "");
        bool startShouldBeUpper = true;
        bool endShouldBeUpper = true;
        //see if the message is 2 or 1 of length, rare but can happen
        int checkLength = Math.Min(3, msgNoSpace.Length);
        for(int i = 1; i < checkLength; i++)
            startShouldBeUpper = startShouldBeUpper && char.IsUpper(msgNoSpace[i]);
        
        for(int i = msgNoSpace.Length - 1; i > msgNoSpace.Length - checkLength; i--)
            endShouldBeUpper = endShouldBeUpper && char.IsUpper(msgNoSpace[i]);

        // Randomly add start
        while (_random.Prob(0.20f))
        {
            var start = _random.Pick(_starts);
            //if the phrase has one of the start words already, don't add it again
            if (!_startwords.Any(word => msg.ToLower().Contains(word) && start.ToLower().Contains(word))) 
            {
                if (startShouldBeUpper)
                    start = start.ToUpper();
                else
                {
                    var first = char.ToLower(msg[0]);
                    var rest = msg.Substring(1);
                    msg = msg.Remove(0);
                    msg = first + rest;
                }
                    
                msg = start + msg;
            }
        }
        // Randomly add ending
        while (_random.Prob(0.20f))
        {
            var ending = _random.Pick(_endings);
            //if the phrase has one of the end words already, don't add it again
            if (!_endwords.Any(word => msg.ToLower().Contains(word) && ending.ToLower().Contains(word))) 
            {
                var lastchar = msg[msg.Length - 1];
                if (lastchar == '.' || lastchar == '!' || lastchar == '?')
                {
                    if (endShouldBeUpper)
                        ending = ending.ToUpper();
                    msg = msg.Remove(msg.Length - 1); //remove punctuation before adding ending
                    msg = msg + ending;

                    if (!((lastchar == '?' || lastchar == '.') && ending.Contains("?")))
                        msg = msg + lastchar;
                }
                else
                {
                    if (endShouldBeUpper)
                        ending = ending.ToUpper();
                    msg = msg + ending;
                }
                    
            }
            
        }

        var words = msg.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (_blockedWords.Any(word => words[i].ToLower().Contains(word)))
                words[i] = words[i] + "|";
        }
        msg = string.Join(' ', words);

        //whatever. go my regexes
        msg = RegexT.Replace(msg, "\'");
        msg = RegexEndT.Replace(msg, "\'");
        msg = RegexNakedH.Replace(msg, "\'");
        msg = ClearBlockedWords.Replace(msg, "");
        return msg;
    }

    private void OnAccent(Entity<BritishAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
