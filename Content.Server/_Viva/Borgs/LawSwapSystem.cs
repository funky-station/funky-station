// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Silicons.Laws;
using Content.Shared._Viva.Silicon;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Server._Viva.Borgs;

public sealed class LawSwapSystem : EntitySystem
{
    [Dependency] private readonly SiliconLawSystem _siliconLawSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconLawBoundComponent, AfterInteractUsingEvent>(WhenLawboardUsed);
        SubscribeLocalEvent<SiliconLawBoundComponent, LawboardDoAfterEvent>(WhenFinishedLawboard);
    }

    private void WhenLawboardUsed(EntityUid uid, SiliconLawBoundComponent lawBoundComp, AfterInteractUsingEvent args)
    {
        if (args.Handled)
            return;

        TryComp<SiliconLawProviderComponent>(args.Used, out var lawBoardProvComp);
        TryComp<WiresPanelComponent>(args.Target, out var wirePanelComp);

        if (lawBoardProvComp == null || wirePanelComp == null)
            return;

        if (wirePanelComp.Open)
        {
            var doafter = new DoAfterArgs(EntityManager, args.User, 25.0f, new LawboardDoAfterEvent(), args.Target, target: args.Target, used: args.Used)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                NeedHand = true,
            };

            if (_doAfterSystem.TryStartDoAfter(doafter))
                args.Handled = true;
        }
        else
        {
            _popupSystem.PopupEntity("You have to open their panel to change their laws!",
                args.User,
                args.User);
            args.Handled = true;
        }
    }

    private void WhenFinishedLawboard(EntityUid entity, SiliconLawBoundComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !args.Target.HasValue)
            return;

        TryComp<SiliconLawProviderComponent>(args.Used, out var lawBoardProvComp);
        TryComp<WiresPanelComponent>(args.Target, out var wirePanelComp);
        if (lawBoardProvComp == null || wirePanelComp == null)
            return;

        if (wirePanelComp.Open)
        {
            _siliconLawSystem.SetLaws(_siliconLawSystem.GetLawset(lawBoardProvComp.Laws).Laws,
                args.Target.Value,
                lawBoardProvComp.LawUploadSound);
            _popupSystem.PopupEntity("You finish reprogramming the borg's laws.",
                args.User,
                args.User);
        }
        else
        {
            _popupSystem.PopupEntity("You have to open their panel to change their laws!",
                args.User,
                args.User);
        }
    }
}
