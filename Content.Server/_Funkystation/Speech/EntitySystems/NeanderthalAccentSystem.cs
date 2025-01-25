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
			EntityManager.TryGetComponent<MetaDataComponent>(uid, out MetaDataComponent? metaDataComponent);
			string name = "";
			if (metaDataComponent != null)
				name = ((MetaDataComponent)metaDataComponent).EntityName;
			
			string[] words = message.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

			string newmessage = "";
			foreach (string word in words)
			{
				if ((name.Length > 0) && (word.ToUpper() == name.ToUpper() ||
					word.ToUpper().StartsWith(name.ToUpper()) || word.ToUpper().EndsWith(name.ToUpper())))
					newmessage = newmessage + " " + name;
				else
					newmessage = newmessage + _random.Pick(Grunts);
			}
			
			if ((message.Length > 1) && (message.EndsWith("!!"))) newmessage = newmessage + "!!";
			else if (message.EndsWith("?") || message.EndsWith("!")) newmessage = newmessage + message[message.Length - 1];
			else newmessage = newmessage + _random.Pick(Terminators);

			return newmessage.Substring(1);
        }

        private void OnAccent(EntityUid uid, NeanderthalAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(uid, args.Message);
        }
		
    }
}
