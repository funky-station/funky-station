using System.Collections;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class FunkyAccentSystem : EntitySystem
    {
        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            { "fuck", "funk" },
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<FunkyAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            var caseBits = new BitArray(message.Length);
            var index = 0;
            foreach (var letter in message)
            {
                if (char.IsUpper(letter))
                    caseBits[index] = true;

                index++;
            }

            foreach (var (word, repl) in SpecialWords)
            {
                message = message.ToLower().Replace(word, repl);
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
