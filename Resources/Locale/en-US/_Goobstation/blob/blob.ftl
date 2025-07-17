# SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
# SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
# SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
# SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

ent-SpawnPointGhostBlob = Blob spawner
    .suffix = DEBUG, Ghost Role Spawner
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Blob Drop
    .desc = A floating, aggressive blob creature that seems to be puffed up with gas. Parasitically zombifies dead organisms by attaching themselves to their head, forcing their body and brain to reanimate.
ent-MobBlobBlobbernaut = Blobbernaut
    .desc = A resilient and powerful blob creature, highly territorial.
ent-BaseBlob = basic blob.
    .desc = { "" }
ent-NormalBlobTile = blob infestation
    .desc = A large swath of blob biomass required for the construction of blob structures. It lashes out at everything around it.
ent-CoreBlobTile = Blob Core
    .desc = The most important part of the blob. By destroying the core, all other parts will die.
ent-FactoryBlobTile = Blob Factory
    .desc = A disgusting looking blob structure that creates blob drops over time, and creates powerful blobbernauts when fed resources.
ent-ResourceBlobTile = Resource Blob
    .desc = A hill-shaped blob structure that constantly produces a yellow viscous fluid. The fluid seems to seep into the surrounding infestation, helping it to spread and grow...
ent-NodeBlobTile = Blob Node
    .desc = A mini version of the core that allows special blob structures to be constructed around itself.
ent-StrongBlobTile = thick blob infestation
    .desc = A raised, reinforced swath of blob biomass. It does not allow air to pass through and protects against kinetic damage.
ent-ReflectiveBlobTile = reflective blob infestation
    .desc = A swath of reflective blob biomass which reflects lasers, but does not protect against kinetic damage as well.
    .desc = { "" }
objective-issuer-blob = Blob


ghost-role-information-blobbernaut-name = Blobbernaut
ghost-role-information-blobbernaut-description = You are a blobbernaut. You must defend the Blob Core at all costs.

ghost-role-information-blob-name = Blob
ghost-role-information-blob-description = You are a Blob. You must consume the station.

roles-antag-blob-name = Blob
roles-antag-blob-objective = Take over the station.

guide-entry-blob = Blob

# Popups
blob-target-normal-blob-invalid = Wrong blob type, select a normal blob.
blob-target-factory-blob-invalid = Wrong blob type, select a factory blob.
blob-target-node-blob-invalid = Wrong blob type, select a node blob.
blob-target-close-to-resource = Too close to another resource blob.
blob-target-nearby-not-node = No node or resource blob nearby.
blob-target-close-to-node = Too close to another node.
blob-target-already-produce-blobbernaut = This factory has already produced a blobbernaut.
blob-cant-split = You can not split the blob core.
blob-not-have-nodes = You have no nodes.
blob-not-enough-resources = Not enough resources.
blob-help = Only God can help you.
blob-swap-chem = In development.
blob-mob-attack-blob = You can not attack a blob.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = You are dying while not on blob tiles.
carrier-blob-alert = You have { $second } seconds left before transformation.

blob-mob-zombify-second-start = { $pod } starts turning you into a zombie.
blob-mob-zombify-third-start = { $pod } starts turning { $target } into a zombie.

blob-mob-zombify-second-end = { $pod } turns you into a zombie.
blob-mob-zombify-third-end = { $pod } turns { $target } into a zombie.

blobberaut-factory-destroy = You are dying due to the destruction of your origin factory.
blob-target-already-connected = This tile is already connected.


# UI
blob-chem-swap-ui-window-name = Swap chemicals
blob-chem-reactivespines-info = Reactive Spines
                                Deals 25 brute damage.
blob-chem-blazingoil-info = Blazing Oil
                            Deals 15 burn damage and lights targets on fire.
                            Makes you vulnerable to water.
blob-chem-regenerativemateria-info = Regenerative Materia
                                    Deals 6 brute damage and 15 toxin damage.
                                    The blob core regenerates health 10 times faster than normal and generates 1 extra resource.
blob-chem-explosivelattice-info = Explosive Lattice
                                    Deals 5 burn damage and explodes the target, dealing 10 brute damage.
                                    Spores explode on death.
                                    You become immune to explosions.
                                    You take 50% more damage from burns and electrical shock.
blob-chem-electromagneticweb-info = Electromagnetic Web
                                    Deals 20 burn damage, 20% chance to cause an EMP pulse when attacking.
                                    Blob tiles cause an EMP pulse when destroyed.
                                    You take 25% more brute and heat damage.

blob-alert-out-off-station = The blob was removed because it was found outside the station!

# Announcment
blob-alert-recall-shuttle = The emergency shuttle can not be sent while there is a level 5 biohazard present on the station.
blob-alert-detect = Confirmed outbreak of level 5 biohazard aboard the station. All personnel must contain the outbreak.
blob-alert-critical = Biohazard level critical, nuclear authentication codes have been sent to the station. Central Command orders any remaining personnel to activate the self-destruction mechanism.
blob-alert-critical-NoNukeCode = Biohazard level critical. Central Command orders any remaining personnel to seek shelter, and await rescue.

# Actions
blob-create-factory-action-name = Place Factory Blob (80)
blob-create-factory-action-desc = Builds a Factory Blob on top of an infestation tile, which will produce up to 3 blob drops if placed next to a core or a node. A blobbernaut can also be created here if fed resources.
blob-create-resource-action-name = Place Resource Blob (60)
blob-create-resource-action-desc = Builds a Resource Blob on top of an infestation tile, which will generates resources if placed next to a core or a node.
blob-create-node-action-name = Place Node Blob (50)
blob-create-node-action-desc = Builds a Node Blob on top of an infestation tile.
                                A node blob will activate effects of factory and resource blobs, heal other blobs and slowly expand, destroying walls and creating normal blobs.
blob-produce-blobbernaut-action-name = Produce a Blobbernaut (60)
blob-produce-blobbernaut-action-desc = Creates a blobbernaut on the selected factory. Each factory can only do this once. The blobbernaut will take damage outside of blob tiles and heal when close to nodes.
blob-split-core-action-name = Split Core (400)
blob-split-core-action-desc = You can only do this once. Turns selected node into an independent core that will act on its own.
blob-swap-core-action-name = Relocate Core (200)
blob-swap-core-action-desc = Swaps the location of your core and the selected node.
blob-teleport-to-core-action-name = Jump to Core (0)
blob-teleport-to-core-action-desc = Teleports you to your Blob Core.
blob-teleport-to-node-action-name = Jump to Node (0)
blob-teleport-to-node-action-desc = Teleports you to a random blob node.
blob-help-action-name = Help
blob-help-action-desc = Get basic information about playing as blob.
blob-swap-chem-action-name = Swap chemicals (70)
blob-swap-chem-action-desc = Lets you swap your current chemical.
blob-carrier-transform-to-blob-action-name = Transform into a blob
blob-carrier-transform-to-blob-action-desc = Instantly destroys your body and creates a blob core. Make sure to stand on a floor tile, otherwise you will simply disappear.
blob-downgrade-action-name = Downgrade Blob tile (0)
blob-downgrade-action-desc = Turns the selected tile back into a normal blob.

# Ghost role
blob-carrier-role-name = Blob carrier
blob-carrier-role-desc =  A blob-infected creature.
blob-carrier-role-rules = You are an antagonist. You have 10 minutes before you automatically transform into a blob.
                        Use this time to find a safe spot on the station. Keep in mind that you will be very weak right after the transformation.
blob-carrier-role-greeting = You are a carrier of the Blob. Find a secluded place on the station and transform into a Blob. Turn the station into a mass, and its inhabitants into your servants.

# Verbs
blob-pod-verb-zombify = Zombify
blob-verb-upgrade-to-strong = Upgrade to Strong Blob
blob-verb-upgrade-to-reflective = Upgrade to Reflective Blob
blob-verb-remove-blob-tile = Remove Blob

# Alerts
blob-resource-alert-name = Core Resources
blob-resource-alert-desc = Your resources produced by the core and resource blobs. Use them to expand and create special blobs.
blob-health-alert-name = Core Health
blob-health-alert-desc = Your core's health. You will die if it reaches zero.

# Greeting
blob-role-greeting =
    You are blob - a parasitic space creature capable of destroying entire stations.
        Your goal is to survive and grow as large as possible.
    	You are almost invulnerable to physical damage, but heat can still hurt you.
        Use Alt+LMB to upgrade normal blob tiles to strong blob and strong blob to reflective blob.
    	Make sure to place resource blobs to generate resources.
        Keep in mind that resource blobs and factories will only work when next to node blobs or cores.
blob-zombie-greeting = You were infected and raised by a blob spore. Now you must help the blob take over the station.

# End round
blob-round-end-result =
    { $blobCount ->
        [one] There was one blob.
        *[other] There were {$blobCount} blobs.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] was a blob.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a blob.
blob-was-a-blob-named = [color=White]{$name}[/color] was a blob.

preset-blob-objective-issuer-blob = [color=#33cc00]Blob[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] was a blob who had the following objectives:
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a blob who had the following objectives:
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] was a blob who had the following objectives:

admin-verb-text-make-blob = Infect the target with the Blob.

# Objectives
objective-condition-blob-capture-title = Take over the station
objective-condition-blob-capture-description = Your only goal is to take over the whole station. You need to have at least {$count} blob tiles.
objective-condition-success = { $condition } | [color={ $markupColor }]Success![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Failure![/color] ({ $progress }%)
