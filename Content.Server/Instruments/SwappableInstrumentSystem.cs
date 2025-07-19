// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Instruments;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;

namespace Content.Server.Instruments;

public sealed class SwappableInstrumentSystem : EntitySystem
{
    [Dependency] private readonly SharedInstrumentSystem _sharedInstrument = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwappableInstrumentComponent, GetVerbsEvent<AlternativeVerb>>(AddStyleVerb);
    }

    private void AddStyleVerb(EntityUid uid, SwappableInstrumentComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || component.InstrumentList.Count <= 1)
            return;

        if (!TryComp<InstrumentComponent>(uid, out var instrument))
            return;

        var priority = 0;
        foreach (var entry in component.InstrumentList)
        {
            AlternativeVerb selection = new()
            {
                Text = entry.Key,
                Category = VerbCategory.InstrumentStyle,
                Priority = priority,
                Act = () =>
                {
                    _sharedInstrument.SetInstrumentProgram(uid, instrument, entry.Value.Item1, entry.Value.Item2);
                    _popup.PopupEntity(Loc.GetString("swappable-instrument-component-style-set", ("style", entry.Key)),
                        args.User, args.User);
                }
            };

            priority--;
            args.Verbs.Add(selection);
        }
    }
}
