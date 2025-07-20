// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Wires;

namespace Content.Server.Wires;

/// <summary>
///     convenience class for wires that depend on the existence of some component to function. Slightly reduces boilerplate.
/// </summary>
public abstract partial class ComponentWireAction<TComponent> : BaseWireAction where TComponent : Component
{
    public abstract StatusLightState? GetLightState(Wire wire, TComponent component);
    public override StatusLightState? GetLightState(Wire wire)
    {
        return EntityManager.TryGetComponent(wire.Owner, out TComponent? component)
            ? GetLightState(wire, component)
            : StatusLightState.Off;
    }

    public abstract bool Cut(EntityUid user, Wire wire, TComponent component);
    public abstract bool Mend(EntityUid user, Wire wire, TComponent component);
    public abstract void Pulse(EntityUid user, Wire wire, TComponent component);

    public override bool Cut(EntityUid user, Wire wire)
    {
        base.Cut(user, wire);
        // if the entity doesn't exist, we need to return true otherwise the wire sprite is never updated
        return EntityManager.TryGetComponent(wire.Owner, out TComponent? component) ? Cut(user, wire, component) : true;
    }

    public override bool Mend(EntityUid user, Wire wire)
    {
        base.Mend(user, wire);
        return EntityManager.TryGetComponent(wire.Owner, out TComponent? component) ? Mend(user, wire, component) : true;
    }

    public override void Pulse(EntityUid user, Wire wire)
    {
        base.Pulse(user, wire);
        if (EntityManager.TryGetComponent(wire.Owner, out TComponent? component))
            Pulse(user, wire, component);
    }
}
