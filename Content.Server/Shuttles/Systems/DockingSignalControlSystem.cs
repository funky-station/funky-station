// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceLinking.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;

namespace Content.Server.Shuttles.Systems;

public sealed class DockingSignalControlSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLinkSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DockingSignalControlComponent, DockEvent>(OnDocked);
        SubscribeLocalEvent<DockingSignalControlComponent, UndockEvent>(OnUndocked);
    }

    private void OnDocked(Entity<DockingSignalControlComponent> ent, ref DockEvent args)
    {
        _deviceLinkSystem.SendSignal(ent, ent.Comp.DockStatusSignalPort, signal: true);
    }

    private void OnUndocked(Entity<DockingSignalControlComponent> ent, ref UndockEvent args)
    {
        _deviceLinkSystem.SendSignal(ent, ent.Comp.DockStatusSignalPort, signal: false);
    }
}
