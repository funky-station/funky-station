// SPDX-FileCopyrightText: 2022 Bright0 <55061890+Bright0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Justin Trotter <trotter.justin@gmail.com>
// SPDX-FileCopyrightText: 2022 Kognise <felix.mattick@gmail.com>
// SPDX-FileCopyrightText: 2022 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Stalen <33173619+stalengd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Internals;
using Content.Shared.Inventory;
using Content.Shared.Roles;

namespace Content.Server.Body.Systems;

public sealed class InternalsSystem : SharedInternalsSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly RespiratorSystem _respirator = default!;

    private EntityQuery<InternalsComponent> _internalsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _internalsQuery = GetEntityQuery<InternalsComponent>();

        SubscribeLocalEvent<InternalsComponent, InhaleLocationEvent>(OnInhaleLocation);
        SubscribeLocalEvent<InternalsComponent, StartingGearEquippedEvent>(OnStartingGear);
    }

    private void OnStartingGear(EntityUid uid, InternalsComponent component, ref StartingGearEquippedEvent args)
    {
        if (component.BreathTools.Count == 0)
            return;

        if (component.GasTankEntity != null)
            return; // already connected

        // Can the entity breathe the air it is currently exposed to?
        if (_respirator.CanMetabolizeInhaledAir(uid))
            return;

        var tank = FindBestGasTank(uid);
        if (tank == null)
            return;

        // Could the entity metabolise the air in the linked gas tank?
        if (!_respirator.CanMetabolizeGas(uid, tank.Value.Comp.Air))
            return;

        ToggleInternals(uid, uid, force: false, component);
    }

    private void OnInhaleLocation(Entity<InternalsComponent> ent, ref InhaleLocationEvent args)
    {
        if (AreInternalsWorking(ent))
        {
            var gasTank = Comp<GasTankComponent>(ent.Comp.GasTankEntity!.Value);
            args.Gas = _gasTank.RemoveAirVolume((ent.Comp.GasTankEntity.Value, gasTank), Atmospherics.BreathVolume);
            // TODO: Should listen to gas tank updates instead I guess?
            _alerts.ShowAlert(ent.Owner, ent.Comp.InternalsAlert, GetSeverity(ent));
        }
    }
}
