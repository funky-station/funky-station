using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

public sealed class ScandinavianAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex RegexLowercaseAe = new(@"ae");
    private static readonly Regex RegexUppercaseAe = new(@"A(?i)e");
    private static readonly Regex RegexLowercaseTh = new(@"th");
    private static readonly Regex RegexUppercaseTh = new(@"T(?i)h");

    public override void Initialize()
    {
        SubscribeLocalEvent<ScandinavianAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        message = RegexLowercaseAe.Replace(message, "æ");
        message = RegexUppercaseAe.Replace(message, "Æ");
        message = RegexLowercaseTh.Replace(message, "ð");
        message = RegexUppercaseTh.Replace(message, "Ð");


        var messageBuilder = new StringBuilder(message);

        // SHITCODE INCOMING. "A" and "O" have a 25% chance (50% * 50%) to be replaced with an accented equivalent. "E" has a 12.5% chance (25% * 50%) to be replaced with "Æ".
        for (var i = 0; i < messageBuilder.Length; i++)
        {
            if (_random.Prob(0.5f))
            {
                var randomInt = _random.Next(0,4);
                switch (messageBuilder[i])
                {
                    case 'A':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'Å',
                            1 => 'Ä',
                            _ => 'A'
                        };
                        break;
                    case 'a':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'å',
                            1 => 'ä',
                            _ => 'a'
                        };
                        break;
                    case 'E':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'Æ',
                            _ => 'E'
                        };
                        break;
                    case 'e':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'æ',
                            _ => 'e'
                        };
                        break;
                    case 'O':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'Ø',
                            1 => 'Ö',
                            _ => 'O'
                        };
                        break;
                    case 'o':
                        messageBuilder[i] = randomInt switch
                        {
                            0 => 'ø',
                            1 => 'ö',
                            _ => 'o'
                        };
                        break;
                }
            }
        }

        return messageBuilder.ToString();
    }
    private void OnAccent(Entity<ScandinavianAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
