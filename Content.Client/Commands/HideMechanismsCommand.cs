// SPDX-FileCopyrightText: 2020 DTanxxx <55208219+DTanxxx@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Body.Organ;
using Robust.Client.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Containers;

namespace Content.Client.Commands;

public sealed class HideMechanismsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override string Command => "hidemechanisms";

    public override string Description => LocalizationManager.GetString($"cmd-{Command}-desc", ("showMechanismsCommand", ShowMechanismsCommand.CommandName));

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var containerSys = _entityManager.System<SharedContainerSystem>();
        var spriteSys = _entityManager.System<SpriteSystem>();
        var query = _entityManager.AllEntityQueryEnumerator<OrganComponent, SpriteComponent>();

        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            spriteSys.SetContainerOccluded((uid, sprite), false);

            var tempParent = uid;
            while (containerSys.TryGetContainingContainer((tempParent, null, null), out var container))
            {
                if (!container.ShowContents)
                {
                    spriteSys.SetContainerOccluded((uid, sprite), true);
                    break;
                }

                tempParent = container.Owner;
            }
        }
    }
}
