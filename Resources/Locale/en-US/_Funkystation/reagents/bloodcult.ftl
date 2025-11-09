# SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

reagent-name-edge-essentia = edge essentia
reagent-desc-edge-essentia = A dark, cursed substance that corrupts the blood of wounded victims, turning their bleeding wounds into sources of sanguine perniculate.

reagent-effect-guidebook-bleed-sanguine-perniculate =
    { $chance ->
        [1] Converts
       *[other] Has a {NATURALPERCENT($chance, 2)} chance to convert
    } bleeding blood into sanguine perniculate

reagent-effect-condition-guidebook-is-blood-cultist = { $invert ->
    [true] the target is not a blood cultist
    *[false] the target is a blood cultist
    }

sanguine-perniculate-holywater-reaction = The unholy blood violently reacts with the holy water, purging itself!
