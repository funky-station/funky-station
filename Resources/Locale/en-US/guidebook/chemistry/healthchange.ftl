# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

health-change-display =
    { $deltasign ->
        [-1] [color=green]{NATURALFIXED($amount, 2)}[/color] {$kind}
        *[1] [color=red]{NATURALFIXED($amount, 2)}[/color] {$kind}
    }
