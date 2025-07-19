// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedStunbatonSystem))]
public sealed partial class StunbatonComponent : Component
{
    [DataField("energyPerUse"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float EnergyPerUse = 350;

    [DataField("sparksSound")]
    public SoundSpecifier SparksSound = new SoundCollectionSpecifier("sparks");
}
