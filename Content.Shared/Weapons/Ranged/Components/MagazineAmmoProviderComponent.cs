// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;

namespace Content.Shared.Weapons.Ranged;

/// <summary>
/// Wrapper around a magazine (handled via ItemSlot). Passes all AmmoProvider logic onto it.
/// </summary>
[RegisterComponent, Virtual]
[Access(typeof(SharedGunSystem))]
public partial class MagazineAmmoProviderComponent : AmmoProviderComponent
{
    [ViewVariables(VVAccess.ReadWrite), DataField("soundAutoEject")]
    public SoundSpecifier? SoundAutoEject = new SoundPathSpecifier("/Audio/Weapons/Guns/EmptyAlarm/smg_empty_alarm.ogg");

    /// <summary>
    /// Should the magazine automatically eject when empty.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("autoEject")]
    public bool AutoEject = false;
}
