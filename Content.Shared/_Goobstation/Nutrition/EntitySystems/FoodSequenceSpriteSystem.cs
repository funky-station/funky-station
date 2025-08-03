// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Utility;


namespace Content.Shared._Goobstation.Nutrition.EntitySystems
{
    public class FoodSequenceSpriteSystem : SharedFoodSequenceSystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<FoodSequenceElementComponent, ComponentStartup>(OnComponentStartup);
        }

        private void OnComponentStartup(Entity<FoodSequenceElementComponent> ent, ref ComponentStartup args)
        {
            if (ent.Comp.Entries.Count == 0)
            {
                var defaultEntry = new FoodSequenceElementEntry();

                if (TryComp<MetaDataComponent>(ent, out var meta))
                {
                    defaultEntry.Name = meta.EntityName.Replace(" ", string.Empty);
                    defaultEntry.Proto = meta.EntityPrototype?.ID;
                }

                ent.Comp.Entries.Add("default", defaultEntry);
            }
        }

    }
}
