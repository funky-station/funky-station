// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Emag.Systems;
using Content.Shared.Popups;
using Content.Shared.Singularity.Components;

namespace Content.Shared.Singularity.EntitySystems;

/// <summary>
/// Shared part of SingularitySingularityGeneratorSystem
/// </summary>
public abstract class SharedSingularityGeneratorSystem : EntitySystem
{
    #region Dependencies
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    #endregion Dependencies

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SingularityGeneratorComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, SingularityGeneratorComponent component, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        if (component.FailsafeDisabled)
            return;

        component.FailsafeDisabled = true;
        args.Handled = true;
    }
}
