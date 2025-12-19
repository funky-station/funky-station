// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DoutorWhite <68350815+DoutorWhite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.Overlays;

public sealed class ShowHealthIconsSystem : EquipmentHudSystem<ShowHealthIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;

    [ViewVariables]
    public HashSet<string> DamageContainers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        SubscribeLocalEvent<ShowHealthIconsComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ShowHealthIconsComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var damageContainerId in component.Components.SelectMany(x => x.DamageContainers))
        {
            DamageContainers.Add(damageContainerId);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        DamageContainers.Clear();
    }

    private void OnHandleState(Entity<ShowHealthIconsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        RefreshOverlay();
    }

    private void OnGetStatusIconsEvent(Entity<DamageableComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive)
            return;

        var healthIcons = DecideHealthIcons(entity);

        args.StatusIcons.AddRange(healthIcons);
    }

    private IReadOnlyList<HealthIconPrototype> DecideHealthIcons(Entity<DamageableComponent> entity)
    {
        var damageableComponent = entity.Comp;

        if (damageableComponent.DamageContainerID == null ||
            !DamageContainers.Contains(damageableComponent.DamageContainerID))
        {
            return Array.Empty<HealthIconPrototype>();
        }

        var result = new List<HealthIconPrototype>();

        if (damageableComponent?.DamageContainerID == "Biological")
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                if (HasComp<RottingComponent>(entity) && _prototypeMan.TryIndex(damageableComponent.RottingIcon, out var rottingIcon))
                {
                    result.Add(rottingIcon);
                }
                else
                {
                    // Try to get the specific state icon
                    if (damageableComponent.HealthIcons.TryGetValue(state.CurrentState, out var value))
                    {
                        if (_prototypeMan.TryIndex(value, out var icon))
                            result.Add(icon);
                    }
                    // Fallback: If in SoftCrit or HardCrit but no icon found, use the standard Critical icon
                    else if ((state.CurrentState == MobState.SoftCritical || state.CurrentState == MobState.HardCritical) &&
                             damageableComponent.HealthIcons.TryGetValue(MobState.Critical, out var critValue))
                    {
                        if (_prototypeMan.TryIndex(critValue, out var critIcon))
                            result.Add(critIcon);
                    }
                }
            }
        }

        return result;
    }
}
