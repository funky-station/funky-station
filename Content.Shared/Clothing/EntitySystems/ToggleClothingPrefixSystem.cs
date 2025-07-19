// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing.Components;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// On toggle handles the changes to ItemComponent.HeldPrefix. <see cref="ToggleClothingPrefixComponent"/>.
/// </summary>
public sealed class ToggleClothingPrefixSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleClothingPrefixComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnToggled(Entity<ToggleClothingPrefixComponent> ent, ref ItemToggledEvent args)
    {
        _clothing.SetEquippedPrefix(ent, args.Activated ? ent.Comp.PrefixOn : ent.Comp.PrefixOff);
    }
}
