// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Shared.CartridgeLoader.Cartridges;

public abstract class SharedNanoTaskCartridgeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoTaskCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
    }

    private void OnCartridgeAdded(Entity<NanoTaskCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<NanoTaskInteractionComponent>(args.Loader);
    }
}
