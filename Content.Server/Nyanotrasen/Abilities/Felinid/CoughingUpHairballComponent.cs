// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.Abilities.Felinid;

[RegisterComponent]
public sealed partial class CoughingUpHairballComponent : Component
{
    [DataField("accumulator")]
    public float Accumulator = 0f;

    [DataField("coughUpTime")]
    public TimeSpan CoughUpTime = TimeSpan.FromSeconds(2.15); // length of hairball.ogg
}
