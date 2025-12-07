// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[RegisterComponent, NetworkedComponent]
public sealed partial class AmmoSelectorComponent : Component
{
    [DataField]
    public HashSet<ProtoId<SelectableAmmoPrototype>> Prototypes = new();

    [DataField]
    public SoundSpecifier? SoundSelect = new SoundPathSpecifier("/Audio/Weapons/Guns/Misc/selector.ogg");
}
