// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Server.Ghost.Roles.Raffles;

/// <summary>
/// Allows getting a <see cref="IGhostRoleRaffleDecider"/> as prototype.
/// </summary>
[Prototype]
public sealed partial class GhostRoleRaffleDeciderPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The <see cref="IGhostRoleRaffleDecider"/> instance that chooses the winner of a raffle.
    /// </summary>
    [DataField("decider", required: true)]
    public IGhostRoleRaffleDecider Decider { get; private set; } = new RngGhostRoleRaffleDecider();
}
