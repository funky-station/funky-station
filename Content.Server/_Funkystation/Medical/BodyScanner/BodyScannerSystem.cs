
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Buckle.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.UserInterface;
using Content.Server.Medical.Components;
using System.Linq;

namespace Content.Server.Medical;

// Is this an EntitySystem?
public sealed partial class BodyScannerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyScannerComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<BodyScannerComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<BodyScannerComponent, AfterActivatableUIOpenEvent>(OnActivateUI);
    }

    private void SetPatient(Entity<BodyScannerComponent> ent, EntityUid patient)
    {
        if (TryComp<HealthAnalyzerComponent>(ent, out var healthAnalyzer))
        {
            healthAnalyzer.ScannedEntity = patient;
        }
    }
    private void UnsetPatient(Entity<BodyScannerComponent> ent)
    {
        if (TryComp<HealthAnalyzerComponent>(ent, out var healthAnalyzer))
        {
            healthAnalyzer.ScannedEntity = null;
        }
    }

    private void OnNewLink(Entity<BodyScannerComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort == ent.Comp.OperatingTablePort &&
            HasComp<OperatingTableComponent>(args.Source))
        {
            ent.Comp.OperatingTable = args.Source;
            //Dirty(ent);
        }
    }

    private void OnPortDisconnected(Entity<BodyScannerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != ent.Comp.OperatingTablePort)
            return;

        ent.Comp.OperatingTable = null;
        UnsetPatient(ent);
        //Dirty(ent);
    }

    private void OnActivateUI(Entity<BodyScannerComponent> ent, ref AfterActivatableUIOpenEvent args)
    //private void OnActivateUI(EntityUID uid, BodyScannerComponent component, ref AfterActivatableUIOpenEvent args)
    {
        if (!TryComp<StrapComponent>(ent.Comp.OperatingTable, out var strap))
            return;

        var buckled = strap.BuckledEntities;
        if (buckled.Count == 0)
            return;

        var patient = buckled.First();
        SetPatient(ent, patient);


    }
}
