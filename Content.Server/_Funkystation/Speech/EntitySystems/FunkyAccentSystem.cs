using System.Collections;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class FunkyAccentSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<FunkyAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            // lowercase everything to make the logic a bit easier.  Keep track of original case to restore after
            // we apply the accent to the message
            var caseBits = new BitArray(message.Length);
            var index = 0;
            foreach (var letter in message)
            {
                if (char.IsUpper(letter))
                    caseBits[index] = true;

                index++;
            }
            message = message.ToLower();

            // Based off of /proc/elvisfy gooncode
            for (var i = 0; i < message.Length;)
            {
                char c = message[i];
                string outMessage = string.Empty;

                char prev = i > 0 ? message[i - 1] : '\0';
                char next = i < message.Length - 1 ? message[i + 1] : '\0';
                char nextNext = i < message.Length - 2 ? message[i + 2] : '\0';
                char nextNextNext = i < message.Length - 3 ? message[i + 3] : '\0';
                char nextNextNextNext = i < message.Length - 4 ? message[i + 4] : '\0';
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

                message = message.Remove(i, used);
                message = message.Insert(i, outMessage);
                i += used;
            }

            var array = message.ToCharArray();
            for (int i = 0; i < caseBits.Length; i++)
            {
                if (caseBits[i])
                {
                    array[i] = char.ToUpper(array[i]);
                }
            }
            message = new string(array);

            return message;
        }

        private void OnAccent(EntityUid uid, FunkyAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
