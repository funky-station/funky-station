// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Francesco <frafonia@gmail.com>
// SPDX-FileCopyrightText: 2022 Jack Fox <35575261+DubiousDoggo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceLinking.Systems;
using Content.Server.Materials;
using Content.Shared.Conveyor;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Destructible;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Physics.Controllers;
using Content.Shared.Power;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Physics.Controllers;

public sealed class ConveyorController : SharedConveyorController
{
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly MaterialReclaimerSystem _materialReclaimer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        UpdatesAfter.Add(typeof(MoverController));
        SubscribeLocalEvent<ConveyorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ConveyorComponent, ComponentShutdown>(OnConveyorShutdown);
        SubscribeLocalEvent<ConveyorComponent, BreakageEventArgs>(OnBreakage);

        SubscribeLocalEvent<ConveyorComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<ConveyorComponent, PowerChangedEvent>(OnPowerChanged);

        base.Initialize();
    }

    private void OnInit(EntityUid uid, ConveyorComponent component, ComponentInit args)
    {
        _signalSystem.EnsureSinkPorts(uid, component.ReversePort, component.ForwardPort, component.OffPort);

        if (PhysicsQuery.TryComp(uid, out var physics))
        {
            var shape = new PolygonShape();
            shape.SetAsBox(0.55f, 0.55f);

            _fixtures.TryCreateFixture(uid, shape, ConveyorFixture,
                collisionLayer: (int) (CollisionGroup.LowImpassable | CollisionGroup.MidImpassable |
                                       CollisionGroup.Impassable), hard: false, body: physics);

        }
    }

    private void OnConveyorShutdown(EntityUid uid, ConveyorComponent component, ComponentShutdown args)
    {
        if (MetaData(uid).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        if (!PhysicsQuery.TryComp(uid, out var physics))
            return;

        _fixtures.DestroyFixture(uid, ConveyorFixture, body: physics);
    }

    private void OnBreakage(Entity<ConveyorComponent> ent, ref BreakageEventArgs args)
    {
        SetState(ent, ConveyorState.Off, ent);
    }

    private void OnPowerChanged(EntityUid uid, ConveyorComponent component, ref PowerChangedEvent args)
    {
        component.Powered = args.Powered;
        UpdateAppearance(uid, component);
        Dirty(uid, component);
    }

    private void UpdateAppearance(EntityUid uid, ConveyorComponent component)
    {
        _appearance.SetData(uid, ConveyorVisuals.State, component.Powered ? component.State : ConveyorState.Off);
    }

    private void OnSignalReceived(EntityUid uid, ConveyorComponent component, ref SignalReceivedEvent args)
    {
        if (args.Port == component.OffPort)
            SetState(uid, ConveyorState.Off, component);

        else if (args.Port == component.ForwardPort)
        {
            SetState(uid, ConveyorState.Forward, component);
        }

        else if (args.Port == component.ReversePort)
        {
            SetState(uid, ConveyorState.Reverse, component);
        }
    }

    private void SetState(EntityUid uid, ConveyorState state, ConveyorComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_materialReclaimer.SetReclaimerEnabled(uid, state != ConveyorState.Off))
            return;

        component.State = state;

        if (state != ConveyorState.Off)
        {
            WakeConveyed(uid);
        }

        UpdateAppearance(uid, component);
        Dirty(uid, component);
    }

    /// <summary>
    /// Awakens sleeping entities on the conveyor belt's tile when it's turned on.
    /// Need this as we might activate under CollisionWake entities and need to forcefully check them.
    /// </summary>
    protected override void AwakenConveyor(Entity<TransformComponent?> ent)
    {
        if (!XformQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        var xform = ent.Comp;

        var beltTileRef = xform.Coordinates.GetTileRef(EntityManager, MapManager);

        if (beltTileRef != null)
        {
            Intersecting.Clear();
            Lookup.GetLocalEntitiesIntersecting(beltTileRef.Value.GridUid, beltTileRef.Value.GridIndices, Intersecting, 0f, flags: LookupFlags.Dynamic | LookupFlags.Sundries | LookupFlags.Approximate);

            foreach (var entity in Intersecting)
            {
                if (!PhysicsQuery.TryGetComponent(entity, out var physics))
                    continue;

                if (physics.BodyType != BodyType.Static)
                    PhysicsSystem.WakeBody(entity, body: physics);
            }
        }
    }
}
