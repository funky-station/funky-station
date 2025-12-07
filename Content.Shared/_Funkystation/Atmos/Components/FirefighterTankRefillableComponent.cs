// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JoulesBerg <104539820+JoulesBerg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;

namespace Content.Shared._Funkystation.Atmos.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FirefighterTankRefillableComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    ///     Name of solution/>.
    /// </summary>
    public const string SolutionName = "tank";

    /// <summary>
    ///     Reagent that will be used in backpack.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> TankReagent = "Water";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public SlotFlags TargetSlot;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntityWhitelist? ProviderWhitelist;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ExternalContainer = false;

    /// <summary>
    ///     Sound played when refilling the backpack.
    /// </summary>
    [DataField]
    public SoundSpecifier FirefightingNozzleRefill = new SoundPathSpecifier("/Audio/Effects/refill.ogg");
}
