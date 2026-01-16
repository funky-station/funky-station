using System.Linq;
using System.Text;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Content.Shared._Funkystation.Genetics.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
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
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GeneticAnalyzerComponent, GeneticAnalyzerDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<GeneticAnalyzerComponent, GeneticAnalyzerPrintMessage>(OnPrint);
    }

    private void OnPrint(EntityUid uid, GeneticAnalyzerComponent component, GeneticAnalyzerPrintMessage args)
    {
        if (string.IsNullOrEmpty(component.PatientName) || !component.Mutations.Any())
            return;

        var discoveredIds = _mutationDiscovery.GetGridDiscovered(uid);

        var printed = EntityManager.SpawnEntity("GeneticAnalyzerReportPaper", Transform(uid).Coordinates);

        _handsSystem.PickupOrDrop(args.Actor, printed, checkActionBlocker: false);

        var random = new Random();
        var reportNumber = random.Next(1, 1000);
        var title = Loc.GetString("genetic-analyzer-report-title", ("number", reportNumber.ToString("D3")));
        _metaData.SetEntityName(printed, title);

        var sb = new StringBuilder();
        sb.AppendLine();
        var sorted = component.Mutations
            .Where(m => m.Block > 0)
            .OrderBy(m => m.Block);

        foreach (var mut in sorted)
        {
            var displayName = discoveredIds.Contains(mut.Id)
                ? mut.Name
                : Loc.GetString("genetic-analyzer-unknown-mutation", ("block", mut.Block));

            sb.AppendLine($"{displayName}:");
            sb.AppendLine(FormatSequence(mut.RevealedSequence));
            sb.AppendLine();
        }

        if (TryComp<PaperComponent>(printed, out var paper))
        {
            _paperSystem.SetContent((printed, paper), sb.ToString().TrimEnd());
        }

        _audioSystem.PlayPvs("/Audio/Machines/short_print_and_rip.ogg", uid,
            AudioParams.Default.WithVariation(0.25f).WithVolume(3f).WithRolloffFactor(2.8f).WithMaxDistance(4.5f));
    }

    private static string FormatSequence(string seq)
    {
        if (string.IsNullOrEmpty(seq))
            return seq;

        const int groupSize = 8;
        var sb = new System.Text.StringBuilder(seq.Length + (seq.Length / groupSize));
        for (int i = 0; i < seq.Length; i++)
        {
            if (i > 0 && i % groupSize == 0)
                sb.Append('-');
            sb.Append(seq[i]);
        }
        return sb.ToString();
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

        _audio.PlayPvs(component.ScanningEndSound, uid);

        _ui.SetUiState(uid, GeneticAnalyzerUiKey.Key, new GeneticAnalyzerUiState(
            patientName: patientMeta.EntityName,
            patientInstability: genetics.GeneticInstability,
            mutations: mutationData,
            discoveredIds: discoveredIds
        ));

        _ui.OpenUi(uid, GeneticAnalyzerUiKey.Key, args.User);
    }
}
