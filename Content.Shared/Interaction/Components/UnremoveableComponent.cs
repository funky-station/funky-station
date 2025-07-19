// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Interaction.Components
{
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class UnremoveableComponent : Component
    {
        /// <summary>
        /// If this is true then unremovable items that are removed from inventory are deleted (typically from corpse gibbing).
        /// Items within unremovable containers are not deleted when removed.
        /// </summary>
        [DataField("deleteOnDrop")]
        public bool DeleteOnDrop = true;
    }
}
