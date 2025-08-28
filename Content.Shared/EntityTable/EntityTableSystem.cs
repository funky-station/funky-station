// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.EntityTable;

public sealed class EntityTableSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public IEnumerable<EntProtoId> GetSpawns(EntityTableSelector? table, System.Random? rand = null)
    {
        if (table == null)
            return new List<EntProtoId>();

        rand ??= _random.GetRandom();
        return table.GetSpawns(rand, EntityManager, _prototypeManager);
    }
}
