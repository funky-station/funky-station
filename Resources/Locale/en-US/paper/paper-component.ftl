# SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
# SPDX-FileCopyrightText: 2023 Eoin Mcloughlin <helloworld@eoinrul.es>
# SPDX-FileCopyrightText: 2023 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2024 dffdff2423 <dffdff2423@gmail.com>
# SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
# SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT


### UI

paper-ui-blank-page-message = This page intentionally left blank

# Shown when paper with words examined details
paper-component-examine-detail-has-words = {CAPITALIZE(THE($paper))} has something written on it.
# Shown when paper with stamps examined
paper-component-examine-detail-stamped-by = {CAPITALIZE(THE($paper))} {CONJUGATE-HAVE($paper)} been stamped by: {$stamps}.

paper-component-action-stamp-paper-other = {CAPITALIZE(THE($user))} stamps {THE($target)} with {THE($stamp)}.
paper-component-action-stamp-paper-self = You stamp {THE($target)} with {THE($stamp)}.

# Indicator to show how full a paper is
paper-ui-fill-level = {$currentLength}/{$maxLength}

paper-ui-save-button = Save ({$keybind})

paper-tamper-proof-modified-message = This page was written using tamper-proof ink.
