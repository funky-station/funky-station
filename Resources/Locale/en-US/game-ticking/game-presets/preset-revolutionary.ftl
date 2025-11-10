# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

## Rev Head

roles-antag-rev-head-name = Head Revolutionary
roles-antag-rev-head-objective = Your objective is to take over the station by converting people to your cause and killing all Command staff on station.

head-rev-role-greeting =
    You are a Head Revolutionary.
    You are tasked with removing all of Command from station via converting, death, exilement or imprisonment.
    The Syndicate has sponsored you with a flash that converts the crew to your side, which may be retrieved from your Uplink using code [color = lightgray]{$code}[/color].
    Beware, this won't work on those wearing flash-protection, or on Mindshielded crew such as Security or Command.
    Viva la revolución!

head-rev-briefing =
    Use flashes to convert people to your cause.
    Eliminate all heads of staff, and secure the station.
    You have been graciously sponsored with an uplink from
    the YLF, in-coordination with the Syndicate.
    Your uplink code is: {$code}

head-rev-break-mindshield = The Mindshield neutralized hypnotic powers, but its functionality has been compromised!

## Rev

roles-antag-rev-name = Revolutionary
roles-antag-rev-objective = Your objective is to ensure the safety and follow the orders of the Head Revolutionaries as well as helping them to convert or get rid of all Command staff on station.

rev-break-control = {$name} has remembered their true allegiance!

rev-lieutenant-greeting = 
    You are a Revolutionary Lieutenant.
    You are able to see your comrades, but are unable to convert anyone.
    Lead your department and co-ordinate with your fellow revolutionaries and head revolutionaries.
    Viva la revolución!

rev-role-greeting =
    You are a Revolutionary.
    You are tasked with taking over the station and protecting the Head Revolutionaries.
    Get rid of all of the Command staff, and listen to your Lieutenants!
    Viva la revolución!

rev-briefing = Help your head revolutionaries convert or get rid of every head to take over the station.

## General

rev-title = Revolutionaries
rev-description = Revolutionaries are among us.

rev-not-enough-ready-players = Not enough players readied up for the game. There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed. Can't start a Revolution.
rev-no-one-ready = No players readied up! Can't start a Revolution.
rev-no-heads = There were no Head Revolutionaries to be selected. Can't start a Revolution.

rev-won = The Head Revs survived and successfully seized control of the station.

rev-lost = Command survived and neutralized all of the Head Revs. Major revolutionary defeat.

rev-stalemate = All of the Head Revs and Command died. It's a major loss on all sides.

rev-reverse-stalemate = Both Command and Head Revs survived. It's a draw.

rev-total-victory = All of Command and Head Revs survived, with all of Command being converted.

rev-headrev-count = {$initialCount ->
    [one] There was one Head Revolutionary:
    *[other] There were {$initialCount} Head Revolutionaries:
}

rev-headrev-name-user = [color=#5e9cff]{$name}[/color] ([color=gray]{$username}[/color]) converted {$count} {$count ->
    [one] person
    *[other] people
}

rev-headrev-name = [color=#5e9cff]{$name}[/color] converted {$count} {$count ->
    [one] person
    *[other] people
}

## Deconverted window

rev-deconverted-title = Deconverted!
rev-deconverted-text =
    As the last headrev was neutralized, the revolution is over.

    You are no longer a revolutionary, so be nice.
rev-deconverted-confirm = Confirm

rev-headrev-must-return = The Revolution is leaderless. We must return to the station within a minute!
rev-headrev-returned = A Head Revolutionary has returned to the station, the Revolution continues!
rev-headrev-abandoned = You have disgraced the revolution by abandoning your station. The Revolution is over.
