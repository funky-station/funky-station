# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

power-radiation-collector-gas-tank-missing = The plasma tank slot is [color=darkred]empty[/color].
power-radiation-collector-gas-tank-present = The plasma tank slot is [color=darkgreen]filled[/color] and the tank indicator reads [color={$fullness ->
    *[0]red]empty
    [1]red]low
    [2]yellow]half-full
    [3]lime]full
}[/color].
power-radiation-collector-enabled = It's switched [color={$state ->
    [true] darkgreen]on
    *[false] darkred]off
}[/color].
