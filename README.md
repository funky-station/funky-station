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

Content contributed to this repository after commit [50f3910abba2a021c5a70b68a65a98bfaf3028c9](https://github.com/funky-station/funky-station/commit/50f3910abba2a021c5a70b68a65a98bfaf3028c9) is licensed under the GNU Affero General Public License version 3.0, unless otherwise stated. See LICENSE-AGPLv3.txt. Content contributed to this repository before commit [b9efd260f3d727b4cc621f189ba96e37f0f30bc5](https://github.com/Goob-Station/Goob-Station/commit/b9efd260f3d727b4cc621f189ba96e37f0f30bc5) is licensed under the MIT license, unless otherwise stated. See LICENSE-MIT.txt.

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
