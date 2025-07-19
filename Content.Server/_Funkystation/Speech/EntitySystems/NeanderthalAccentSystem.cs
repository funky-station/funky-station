// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Speech.Components;
using Robust.Shared.Random;
using Robust.Shared.GameObjects;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class NeanderthalAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly IReadOnlyList<string> Grunts = new List<string>{
            " urg", " wuh", " gruh", " ung", " UNG", " buh-buh", " grrrrh", " GRRRRH", "grrrh",
			" grrh", " unga", " UNGA", " bunga", " grunga", " guhhh", " guh", " hooh"
        }.AsReadOnly();

		private static readonly IReadOnlyList<string> Terminators = new List<string>{
			"!", "!!!", "...?", "....", "?!!", "."
		}.AsReadOnly();

        public override void Initialize()
        {
            SubscribeLocalEvent<NeanderthalAccentComponent, AccentGetEvent>(OnAccent);
        }

		
        public string Accentuate(EntityUid uid, string message)
        {
			string name = "";
			if (TryComp<MetaDataComponent>(uid, out var metaDataComponent))
				name = metaDataComponent.EntityName;
			
			string[] words = message.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

			string newMessage = "";
			foreach (string word in words)
			{
				if ((name.Length > 0) && (word.ToUpper() == name.ToUpper() ||
					word.ToUpper().StartsWith(name.ToUpper()) || word.ToUpper().EndsWith(name.ToUpper())))
					newMessage = newMessage + " " + name;
				else
					newMessage = newMessage + _random.Pick(Grunts);
			}
			
			if ((message.Length > 1) && (message.EndsWith("!!")))
				newMessage = newMessage + "!!";
			else if (message.EndsWith("?") || message.EndsWith("!"))
				newMessage = newMessage + message[message.Length - 1];
			else
				newMessage = newMessage + _random.Pick(Terminators);

			return newMessage.Substring(1);
        }

        private void OnAccent(EntityUid uid, NeanderthalAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(uid, args.Message);
        }
		
    }
}
