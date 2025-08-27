// SPDX-FileCopyrightText: 2025 mnva <218184747+mnva0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Buckle.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.UserInterface;
using Content.Server.Medical.Components;
using System.Linq;

namespace Content.Server.Medical;

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
            healthAnalyzer.ScannedEntity = patient;
    }

    private void UnsetPatient(Entity<BodyScannerComponent> ent)
    {
        if (TryComp<HealthAnalyzerComponent>(ent, out var healthAnalyzer))
            healthAnalyzer.ScannedEntity = null;
    }

    private void OnNewLink(Entity<BodyScannerComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort == ent.Comp.OperatingTablePort &&
            HasComp<OperatingTableComponent>(args.Source))
        {
            ent.Comp.OperatingTable = args.Source;
        }
    }

    private void OnPortDisconnected(Entity<BodyScannerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != ent.Comp.OperatingTablePort)
            return;

        ent.Comp.OperatingTable = null;
        UnsetPatient(ent);
    }

    private void OnActivateUI(Entity<BodyScannerComponent> ent, ref AfterActivatableUIOpenEvent args)
    {
        if (!TryComp<StrapComponent>(ent.Comp.OperatingTable, out var strap))
            return;

        var buckled = strap.BuckledEntities;
        if (buckled.Count == 0)
            UnsetPatient(ent);

        var patient = buckled.FirstOrDefault();
        SetPatient(ent, patient);


    }
}
