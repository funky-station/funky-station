// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared.Item.ItemToggle;

/// <summary>
/// On toggle handles the changes to ItemComponent.HeldPrefix. <see cref="ItemTogglePrefixComponent"/>.
/// </summary>
public sealed class ItemTogglePrefixSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemTogglePrefixComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnToggled(Entity<ItemTogglePrefixComponent> ent, ref ItemToggledEvent args)
    {
        _item.SetHeldPrefix(ent.Owner, args.Activated ? ent.Comp.PrefixOn : ent.Comp.PrefixOff);
    }
}
