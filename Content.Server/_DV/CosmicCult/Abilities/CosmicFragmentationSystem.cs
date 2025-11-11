// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Carrot <carpecarrot@gmail.com>
// SPDX-FileCopyrightText: 2025 Currot <carpecarrot@gmail.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._DV.Objectives.Events;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Shared.Popups;
using Content.Server.Radio.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.Radio;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicFragmentationSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!; // Funky

    private ProtoId<RadioChannelPrototype> _cultRadio = "CosmicRadio";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AILawUpdatedEvent>(OnLawInserted);

        SubscribeLocalEvent<BorgChassisComponent, MalignFragmentationEvent>(OnFragmentBorg);
        SubscribeLocalEvent<SiliconLawUpdaterComponent, MalignFragmentationEvent>(OnFragmentAi);

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentation>(OnCosmicFragmentation);
    }

    private void UnEmpower(Entity<CosmicCultComponent> ent)
    {
        var comp = ent.Comp;
        comp.CosmicEmpowered = false; // empowerment spent! Now we set all the values back to their default.
        comp.CosmicSiphonQuantity = CosmicCultComponent.DefaultCosmicSiphonQuantity;
        comp.CosmicGlareRange = CosmicCultComponent.DefaultCosmicGlareRange;
        comp.CosmicGlareDuration = CosmicCultComponent.DefaultCosmicGlareDuration;
        comp.CosmicGlareStun = CosmicCultComponent.DefaultCosmicGlareStun;
        comp.CosmicImpositionDuration = CosmicCultComponent.DefaultCosmicImpositionDuration;
        comp.CosmicBlankDuration = CosmicCultComponent.DefaultCosmicBlankDuration;
        comp.CosmicBlankDelay = CosmicCultComponent.DefaultCosmicBlankDelay;
        _actions.RemoveAction(ent.Owner, comp.CosmicFragmentationActionEntity);
        comp.CosmicFragmentationActionEntity = null;
    }

    private void OnCosmicFragmentation(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentation args)
    {
        if (args.Handled || HasComp<ActiveNPCComponent>(args.Target) || _mobStateSystem.IsIncapacitated(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), ent, ent);
            return;
        }
        var evt = new MalignFragmentationEvent(ent, args.Target);
        RaiseLocalEvent(args.Target, ref evt);
        if (evt.Cancelled) return;
        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("cosmicability-fragmentation-success", ("user", ent), ("target", args.Target)), ent, PopupType.MediumCaution);
        _cult.MalignEcho(ent);
        UnEmpower(ent);
    }

    private void OnFragmentBorg(Entity<BorgChassisComponent> ent, ref MalignFragmentationEvent args)
    {
        // Begin Funky changes
        if (!_mind.TryGetMind(args.Target, out var mindId, out var mind) || _cultRule.AssociatedGamerule(args.User) is not { } cult)
        {
            args.Cancelled = true;
            return;
        }
        if (cult.Comp.ChantryActive)
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-chantry-already-present"), args.User, args.User);
            args.Cancelled = true;
            return;
        }
        _cultRule.SetChantryActive(args.User, true); // Prevent multiple chantries from being created at once
        var wisp = Spawn("CosmicChantryWisp", Transform(args.Target).Coordinates);
        var chantry = Spawn("CosmicBorgChantry", Transform(args.Target).Coordinates);
        _cultRule.TransferCultAssociation(args.User, chantry);
        EnsureComp<CosmicChantryComponent>(chantry, out var chantryComponent);
        // End Funky changes
        chantryComponent.InternalVictim = wisp;
        chantryComponent.VictimBody = args.Target;
        _mind.TransferTo(mindId, wisp, mind: mind);

        var mins = chantryComponent.EventTime.Minutes;
        var secs = chantryComponent.EventTime.Seconds;
        _antag.SendBriefing(wisp, Loc.GetString("cosmiccult-silicon-chantry-briefing", ("minutesandseconds", $"{mins} minutes and {secs} seconds")), Color.FromHex("#4cabb3"), null);
    }

    private void OnFragmentAi(Entity<SiliconLawUpdaterComponent> ent, ref MalignFragmentationEvent args)
    {
        var lawboard = Spawn("CosmicCultLawBoard", Transform(args.Target).Coordinates);
        _container.TryGetContainer(args.Target, "circuit_holder", out var container);
        if (container == null)
            return;
        _container.EmptyContainer(container, true);
        _container.Insert(lawboard, container, Transform(args.Target), true);
    }

    private void OnLawInserted(AILawUpdatedEvent args)
    {
        if (!TryComp<IntrinsicRadioTransmitterComponent>(args.Target, out var radio) || !TryComp<ActiveRadioComponent>(args.Target, out var transmitter))
            return;
        if (args.Lawset.Id == "CosmicCultLaws")
        {
            radio.IntrinsicChannels.Add(_cultRadio);
            transmitter.IntrinsicChannels.Add(_cultRadio);
            _antag.SendBriefing(args.Target, Loc.GetString("cosmiccult-ai-subverted-briefing"), Color.FromHex("#4cabb3"), null);
        }
        else
        {
            radio.IntrinsicChannels.Remove(_cultRadio);
            transmitter.IntrinsicChannels.Remove(_cultRadio);
        }
    }
}

[ByRefEvent]
public record struct MalignFragmentationEvent(Entity<CosmicCultComponent> User, EntityUid Target, bool Cancelled = false);
