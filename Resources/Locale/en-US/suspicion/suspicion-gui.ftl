# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

## SuspicionGui.xaml.cs

# Shown when clicking your Role Button in Suspicion
suspicion-ally-count-display = {$allyCount ->
    *[zero] You have no allies
    [one] Your ally is {$allyNames}
    [other] Your allies are {$allyNames}
}