using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Content.Shared._Funkystation.Genetics.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.GeneticAnalyzer;

public sealed class GeneticAnalyzerSystem : SharedGeneticAnalyzerSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedMutationDiscoverySystem _mutationDiscovery = default!;
    [Dependency] private readonly PowerCellSystem _cell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GeneticAnalyzerComponent, GeneticAnalyzerDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, GeneticAnalyzerComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { Valid: true } target || !args.CanReach)
            return;

        if (!_cell.HasDrawCharge(uid, user: args.User))
        {
            _popup.PopupEntity(Loc.GetString("health-analyzer-popup-no-power"), uid, args.User);
            return;
        }

        if (!TryComp<GeneticsComponent>(target, out _))
            return;

        args.Handled = true;

        var doAfter = new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(1.5f), new GeneticAnalyzerDoAfterEvent(), uid, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTarget
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(EntityUid uid, GeneticAnalyzerComponent component, GeneticAnalyzerDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is not { Valid: true } target || !TryComp<GeneticsComponent>(target, out var genetics))
            return;

        if (!_cell.HasDrawCharge(uid))
        {
            _popup.PopupEntity(Loc.GetString("health-analyzer-popup-no-power"), uid, args.User);
            return;
        }

        var patientMeta = MetaData(target);

        var mutationData = genetics.Mutations
            .Where(m => m.Block > 0)
            .ToList();

        var discoveredIds = _mutationDiscovery.GetGridDiscovered(uid);

        SetScanResults(uid, patientMeta.EntityName, genetics.GeneticInstability, mutationData);

        _audio.PlayPvs("/Audio/Items/Medical/healthscanner.ogg", uid);

        _ui.SetUiState(uid, GeneticAnalyzerUiKey.Key, new GeneticAnalyzerUiState(
            patientName: patientMeta.EntityName,
            patientInstability: genetics.GeneticInstability,
            mutations: mutationData,
            discoveredIds: discoveredIds
        ));

        _ui.OpenUi(uid, GeneticAnalyzerUiKey.Key, args.User);
    }
}
