using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class OkayAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OkayAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, OkayAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "okay");

        // Prefix
        if (_random.Prob(0.4f))
        {
            var pick = _random.Next(1, 7);

            // Reverse sanitize capital
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-okay-prefix-{pick}") + " " + message;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.6f))
        {
            var pick = _random.Next(1, 2);
            message += Loc.GetString($"accent-okay-suffix-{pick}");
        }

        args.Message = message;
    }
};
