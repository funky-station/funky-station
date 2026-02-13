// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2026 ALooseGoose <ALooseGoosey@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.CloneProjector;

namespace Content.Server._Goobstation.CloneProjector;

[RegisterComponent]
public sealed partial class WearingCloneProjectorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? ConnectedProjector;
}
