# SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

roles-antag-cult-name = Blood Cultist
roles-antag-cult-objective = Your objective is to summon your master, the Geometer of Blood, Nar'Sie. Through conversion, domination, and blood may She be brought back into this world. Cooperate with your fellow Cultists to tear open the veil and bring about Her coming!

cult-role-greeting =
    You are a Blood Cultist.
    You are tasked with summoning your master, Nar'Sie, to this plane of existence.
    Sacrifice those the Geometer of Blood demands and convert your crewmates to the cause to tear open the veil.
    TOK-LYR RQA-NAP G'OLT-ULOFT!!

cult-briefing = Help your fellow cult members convert the crew and sacrifice your targets to summon your deity.

cult-start-briefing = The Geometer of Blood has a task for you.

cult-briefing-targets = Nar'Sie demands blood. Inspect the veil to comprehend Her wishes.

admin-verb-make-cultist = Make the target into a Blood Cultist.

cult-dagger-equip-fail = The dagger turns to ash in your hands!

cult-attack-repelled = Holy magic repels your attack!
cult-attack-teamhit = Your attack stops dead before hitting a cultist.

contraband-examine-text-BloodCult = [color=crimson]This item is a highly illegal product of blood cultist magic![/color]

cult-rune-drawing-novowel = You begin smearing a rune into the floor with blood...
cult-rune-drawing-vowel-first = You begin smearing a
cult-rune-drawing-vowel-second = into the floor with blood...
cult-rune-select = Select Rune

cult-veil-drawing-toostrong = The veil is too strong here to tear open.
cult-veil-drawing-pleaseconfirm = Use the dagger again to confirm the {$name} area -- beware, the crew will be alerted!
cult-veil-drawing-wronglocation = You must draw this rune at the {$name} area!
cult-veil-drawing-alreadyexists = Somebody has already drawn a tear veil rune!
cult-veil-drawing-crewwarning = Figments from an eldritch god are being summoned into the {$name} area from an unknown dimension. Disrupt this ritual at all costs, before the station is destroyed. Space Law and SOP are hereby suspended. The entire crew must kill cultists on sight.

cult-invocation-blood-drain = You feel your veins narrow as your blood drains!
cult-invocation-revive-fail = Nar'Sie demands more sacrifice!
cult-invocation-fail-nosoul = Nar'Sie rejects this soulless husk!
cult-invocation-fail-teamkill = Nar'Sie rejects your offering of another Cultist!
cult-invocation-fail-mindshielded = Your victim resists Nar'Sie's influence!
cult-invocation-fail-resisted = This holy being resists Nar'Sie's influence!
cult-invocation-fail = More cultists must be present!
cult-invocation-target-fail = More cultists must be present to sacrifice one Nar'Sie desires!
cult-invocation-narsie-fail = At least nine cultists must stand atop the rune, distributed evenly, to rend the veil.

cult-invocation-barrier = Khari'd! Eske'te tannin!
cult-invocation-revive = Pasnar val'keriam usinar. Savrae ines amutan. Yam'toth remium il'tarat!
cult-invocation-offering = Mah'weyh pleggh at e'ntrath!
cult-invocation-empowering = H'drak v'loso, mir'kanas verbot!

cult-spell-carving = Otherworldly tendrils begin crudely carving a sigil into your flesh.
cult-spell-carving-rune = Empowered by the rune, you barely feel the sigil being carved into your flesh.
cult-spell-exceeded = You cannot carve another spell!
cult-spell-havealready = You already have that spell!
cult-spell-fail = You fail to cast the spell!
cult-spell-repelled = Holy magic protects your target!

cult-shade-summoned = The stolen soul materializes as a servant Shade!
cult-shade-recalled = You recapture the stolen soul, rejuvinating it!
cult-shade-servant = You have been released from your prison, but you are still bound to {$name}'s will. Help them succeed in their goals at all costs.
cult-shade-death-return = The shade's essence returns to the soulstone.

cult-soulstone-empty = You are unable to contact any soul from this stone -- perhaps it is empty.

cult-status-veil-strong = [italic]The Veil needs to be weakened before we are able to summon The Dark One.[/italic]

cult-status-veil-weak = [italic]You and your acolytes have succeeded in preparing the station for the ultimate ritual![/italic]
cult-veil-torn = The veil... is... torn!

cult-status-veil-strong-goal = [italic]Current goal: Sacrifice {$targetName}, the {$targetJob} via invoking an offer rune with its body or brain on it and at least {$cultistsRequired} cultists around it.[/italic]

cult-status-veil-weak-goal = [italic]Current goal: Summon Nar'Sie by invoking the rune 'Tear Veil' with 9 cultists, constructs, or summoned ghosts on it.
    The summoning can only be accomplished in the {$firstLoc} area, the {$secondLoc} area, or the {$thirdLoc} area - where the veil is weak enough for the ritual to begin.[/italic]

cult-status-veil-weak-cultdata = Current cult members: {$cultCount} | Conversions until Rise: {$cultUntilRise}
    Cultists: {$cultistCount}
    Constructs: {$constructCount}

cult-narsie-sacrifice-accept = "I accept your sacrifice."
cult-narsie-target-down = "Yes! This is the one I desire! You have done well."

cult-ascend-1 = The veil weakens as your cult grows, and your eyes begin to glow...
cult-ascend-2 = The veil weakens as your cult grows, and you are unable to hide your true nature!

cult-deconverted = You suddenly de-convert, and no longer consider yourself a cultist!

cult-commune-window = Commune
cult-commune-info = Whisper into the veil and communicate with your fellow worshippers.
cult-commune-send = Send
cult-commune-message = Acolyte {$name} (as {$job}): {$message}
cult-commune-incantation = Y'll tor tz'ul z'nik rar.

cult-narsie-spawning = Reality breaks down around you.

cult-win-announcement-shuttle-call = Due to spatio-temporal complications, the station has been deemed unprofitable to salvage. A crew transfer shuttle has been dispatched. Failure to board and return to Central Command for debriefing will be interpreted as abandonment of contract, and your families will undergo penalties for as long as it takes to recoup these expenses.
    ETA: {$time} {$units}.
cult-win-announcement = Due to spatio-temporal complications, the station has been deemed unprofitable to salvage. Failure to return to Central Command for debriefing will be interpreted as abandonment of contract, and your families will undergo penalties for as long as it takes to recoup these expenses.

cult-ghost-role-name = Reawakened Blood Cultist
cult-ghost-role-desc = A zealous Blood Cultist of Nar'Sie, re-awakened after catatonia.
cult-ghost-role-rules = You are a team antagonist. Work with your fellow Cultists to accomplish your goals.
                        Sacrifice the crewmembers your deity craves and render the veil that binds Her asunder.

cult-roundend-victory = The blood cult has summoned Nar'Sie and laid claim
    to the sector in Her name.
cult-roundend-failure = The blood cult was unable to summon their master.
cult-roundend-count = There were {$count} total blood cultists.
cult-roundend-sacrifices = The blood cult rendered {$sacrifices} souls up to the Geometer of Blood.

cult-soulstone-role-name = Trapped Soul
cult-soulstone-role-description = You are trapped in a soulstone. You can speak and will be released as a shade when a cultist uses the stone.
cult-soulstone-role-rules = You are serving the blood cult. Follow the orders of the cultist who holds your soulstone and help them achieve their goals.