using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class PrickAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrickAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, PrickAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "prick");

        // Prefix
        if (_random.Prob(0.2f))
        {
            var pick = _random.Next(1, 5);

            // Reverse sanitize capital
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-prick-prefix-{pick}") + " " + message;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 4);
            message += Loc.GetString($"accent-prick-suffix-{pick}");
        }

        args.Message = message;
    }
};
