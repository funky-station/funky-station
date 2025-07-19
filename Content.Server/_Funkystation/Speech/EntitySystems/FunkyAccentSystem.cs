// SPDX-FileCopyrightText: 2025 empty0set <16693552+empty0set@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 empty0set <empty0set@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class FunkyAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private readonly IReadOnlyList<string> _endings = new List<string>(){ ", uh huh.", ", alright?", ", mmh"};

        public override void Initialize()
        {
            SubscribeLocalEvent<FunkyAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            // Based off of /proc/elvisfy gooncode
            for (var i = 0; i < message.Length;)
            {
                // we lose some finesse by only accounting for the case of the first letter, so anything mixed case
                // isn't going to be retained, but I'm sure the VAST majority of words are going to be all or nothing in
                // terms of capitalization.
                bool isCapital = char.IsUpper(message[i]);
                // Except for the first letter of a sentence d'oh!
                bool isNextCapital = i < message.Length - 1 ? char.IsUpper(message[i + 1]) : false;

                char c = char.ToLower(message[i]);

                string outMessage = string.Empty;
                #region SHITCODE
                char prev = i > 0 ? message[i - 1] : '\0';
                char next = i < message.Length - 1 ? char.ToLower(message[i + 1]) : '\0';
                char nextNext = i < message.Length - 2 ? char.ToLower(message[i + 2]) : '\0';
                char nextNextNext = i < message.Length - 3 ? char.ToLower(message[i + 3]) : '\0';
                #endregion

                int used = 0; // sometimes this isn't the length of the replacement string.  Don't ask me why.

                switch (c)
                {
                    case 'f':
                    {
                        if (next == 'u' && nextNext == 'c' && nextNextNext == 'k')
                        {
                            outMessage = "funk";
                            used = 4;
                        }

                        break;
                    }
                    case 't':
                    {
                        if (next == 'i' && nextNext == 'o' && nextNextNext == 'n')
                        {
                            outMessage = "shun";
                            used = 4;
                        }
                        else if (next == 'h' && nextNext == 'e')
                        {
                            outMessage = "tha";
                            used = 3;
                        }
                        else if (next == 'h' &&
                                 (nextNext == ' ' || nextNext == ',' || nextNext == '.' || nextNext == '-'))
                        {
                            outMessage = "t" + nextNext;
                            used = 3;
                        }

                        break;
                    }
                    case 'u':
                    {
                        if (prev != ' ' || next != ' ')
                        {
                            outMessage = "uh";
                            used = 2;
                        }

                        break;
                    }
                    case 'o':
                    {
                        if (next == 'w' && (prev != ' ' || nextNext != ' '))
                        {
                            outMessage = "aw";
                            used = 2;
                        }
                        else if (prev != ' ' || next != ' ')
                        {
                            outMessage = "ah";
                            used = 1;
                        }

                        break;
                    }
                    case 'i':
                    {
                        if (next == 'r' && (prev != ' ' || nextNext != ' '))
                        {
                            outMessage = "ahr";
                            used = 2;
                        }
                        else if (next == 'n' && nextNext == 'g')
                        {
                            outMessage = "in'";
                            used = 3;
                        }

                        break;
                    }
                    case 'e':
                    {
                        if (next == 'n' && nextNext == ' ')
                        {
                            outMessage = "un ";
                            used = 3;
                        }

                        if (next == 'r' && nextNext == ' ')
                        {
                            outMessage = "ah ";
                            used = 3;
                        }
                        else if (next == 'w' && (prev != ' ' || nextNext != ' '))
                        {
                            outMessage = "yew";
                            used = 2;
                        }
                        else if (next == ' ' && prev == ' ') //!!!
                        {
                            outMessage = "ee";
                            used = 1;
                        }

                        break;
                    }
                    case 'a':
                    {
                        if (next == 'u')
                        {
                            outMessage = "ah";
                            used = 2;
                        }
                        else if (next == 'n')
                        {
                            outMessage = "ain";
                            used = nextNext == 'd' ? 3 : 2;
                        }

                        break;
                    }
                }

                if (outMessage == string.Empty)
                {
                    used = 1;
                    outMessage = c.ToString();
                }

                // As a heuristic if what we are replacing only starts with a single capital letter don't capitalize the
                // whole thing.  This way starting a sentence like 'And' will be replaced with 'Ain', instead of 'AIN'
                if (isCapital && isNextCapital)
                    outMessage = outMessage.ToUpper();
                if (isCapital && !isNextCapital)
                {
                    char[] array = outMessage.ToCharArray();
                    array[0] = char.ToUpper(array[0]);
                    outMessage = new string(array);
                }

                message = message.Remove(i, used);
                message = message.Insert(i, outMessage);
                i += used;
            }

            if (_random.Prob(0.15f))
                message += _random.Pick(_endings);

            return message;
        }

        private void OnAccent(EntityUid uid, FunkyAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
