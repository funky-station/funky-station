// SPDX-FileCopyrightText: 2024 SpeltIncorrectyl <66873282+SpeltIncorrectyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Power.Generator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveGeneratorRevvingComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan CurrentTime = TimeSpan.Zero;
}
