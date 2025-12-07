// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Shared._Funkystation.Implants;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Implants.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server._Funkystation.Implants
{
    public sealed class ReagentImplantSystem : SharedReagentImplantSystem
    {
        [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SubdermalImplantComponent, UseReagentImplantEvent>(OnReagentImplant);
        }

        // This implant has an internal solution storage, which is transferred from the implanter to the subdermal implant when implanted.
        // Using the implant's ability transfers 15u of the implant's solution to the user's bloodstream.
        private void OnReagentImplant(EntityUid uid, SubdermalImplantComponent comp, UseReagentImplantEvent args)
        {
            var user = args.Performer;
            if (!HasComp<SolutionContainerManagerComponent>(uid) ||
                !_solution.TryGetSolution(uid, "drink", out var _, out var solution) ||
                solution.Volume == 0)
            {
                _popup.PopupEntity(Loc.GetString("reagent_implant_inject_empty"), user, user);
                return;
            }

            if (!_solution.TryGetInjectableSolution(user, out var targetSolution, out var _) ||
                !_solution.TryTransferSolution(targetSolution.Value, solution, 15))
            {
                _popup.PopupEntity(Loc.GetString("reagent_implant_inject_fail"), user, user);
                return;
            }

            _popup.PopupEntity(Loc.GetString("reagent_implant_inject"), user, user);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/hypospray.ogg"), user);
            args.Handled = true;
        }
    }

}
