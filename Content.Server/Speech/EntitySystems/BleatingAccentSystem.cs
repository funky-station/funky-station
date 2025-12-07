// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class BleatingAccentSystem : EntitySystem
{
    private static readonly Regex BleatRegex = new("([mbdlpwhrkcnytfo])([aiu])", RegexOptions.IgnoreCase);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BleatingAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    private void OnAccentGet(Entity<BleatingAccentComponent> entity, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }

    public static string Accentuate(string message)
    {
        // Repeats the vowel in certain consonant-vowel pairs
        // So you taaaalk liiiike thiiiis
        return BleatRegex.Replace(message, "$1$2$2$2$2");
    }
}
