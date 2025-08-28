# SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

$replacements = @{
    "moonheart08" = "moony"
    "Elijahrane" = "Rane"
    "ZeroDayDaemon" = "Daemon"
    "ElectroJr" = "ElectroSR"
    "Partmedia" = "notafet"
    "Just-a-Unity-Dev" = "eclips_e"
}

$ignore = @{
    "funkystationbot" = $true
    "github-actions[bot]" = $true
    "ZDDM" = $true
    "TYoung86" = $true
    "paul" = $true # erroneously included -- presumably from PaulRitter, somehow, who is already credited
    "08a" = $true # erroneously included -- valid github account, but not an actual contributor, probably an alias of a contributor who does not own this github account and is already credited somewhere.
    "UristMcContributor" = $true # this was an account used to demonstrate how to create a valid PR, and is in actuality Willhelm53, who is already credited.
}

$add = @("RamZ")
