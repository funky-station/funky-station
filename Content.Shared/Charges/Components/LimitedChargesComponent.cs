// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Amethyst <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tobias Berger <toby@tobot.dev>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Charges.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Charges.Components;

/// <summary>
/// Specifies the attached action has discrete charges, separate to a cooldown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedChargesSystem))]
public sealed partial class LimitedChargesComponent : Component
{
    [DataField, AutoNetworkedField]
    public int LastCharges;

    /// <summary>
    ///     The max charges this action has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxCharges = 3;

    /// <summary>
    /// Last time charges was changed. Used to derive current charges.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan LastUpdate;
}
