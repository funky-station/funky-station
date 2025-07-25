# SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

-damage-popup-component-type =
    { $setting ->
        [combined] Combined
        [total] Total
        [delta] Delta
        [hit] Hit
       *[other] Unknown
    }

damage-popup-component-switched = Target set to type: { -damage-popup-component-type(setting: $setting) }