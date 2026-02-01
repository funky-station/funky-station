// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Medical;

[RegisterComponent, NetworkedComponent]
public sealed partial class BedWheelsComponent : Component
{
    [DataField]
    public bool Locked = true;
}
