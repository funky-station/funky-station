<!--
SPDX-FileCopyrightText: 2017 PJB3005 <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2019 Ivan <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2019 Silver <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2020 Injazz <43905364+Injazz@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 RedlineTriad <39059512+RedlineTriad@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
SPDX-FileCopyrightText: 2023 iglov <iglov@avalon.land>
SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
SPDX-FileCopyrightText: 2024 router <messagebus@vk.com>
SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>

SPDX-License-Identifier: MIT
-->

<p align="center"> <img alt="Space Station 14" width="880" height="200" src="https://github.com/funky-station/funky-station/blob/master/Resources/Textures/Logo/logo.png" /></p>

This is a fork from the primary repo for Goob Station 14 called Funky Station. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server.

If you want to host or create content for SS14, or for Goob Station, go to the [Space Station 14 repository](https://github.com/space-wizards/space-station-14), or the [Goob Station repository](https://github.com/Goob-Station/Goob-Station).

## Links

[Funky Station Discord Server](https://discord.gg/5FqgaAA2qF)

## Documentation/Wiki

The Goob Station [docs site](https://docs.goobstation.com/) has documentation on GS14's content, engine, game design, and more. It also have lots of resources for new contributors to the project.

## Contributing

We welcome everyone to contribute to our fork. Please join our Discord for collaborating!
We recommend you read the contribution guidelines. [Contribution Guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html)

## Building

We provide some scripts shown below to make the job easier.

### Build dependencies

> - [Git](https://git-scm.com)
> - [.NET SDK 9.0.101](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Windows

> 1. Clone this repository
> 2. Run `Scripts/bat/buildAllDebug.bat` after making any changes to the source
> 3. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 4. Connect to localhost in the client and play

### Linux

> 1. Clone this repository
> 2. Run `Scripts/sh/buildAllDebug.sh` after making any changes to the source
> 3. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 4. Connect to localhost in the client and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux

If your changes are under the resources folder, you do not need to build more than once, only run.

[More detailed instructions on building the project.](https://docs.goobstation.com/en/general-development/setup.html)

## License

All code in this codebase is released under the AGPL-3.0-or-later license. Each file includes REUSE Specification headers or separate .license files that specify a dual license option. This dual licensing is provided to simplify the process for projects that are not using AGPL, allowing them to adopt the relevant portions of the code under an alternative license. You can review the complete texts of these licenses in the LICENSES/ directory.

Most media assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.

If you find that your work is misattributed or someone elses work is misattributed, please create an issue on this repos GitHub page, or email the Funky Station Maintainers @ `maintainers@funkystation.org`.
