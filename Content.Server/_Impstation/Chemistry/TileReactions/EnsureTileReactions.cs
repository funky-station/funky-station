// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.TileReactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class EnsureTileReaction : ITileReaction
    {
        [DataField, ViewVariables]
        public ComponentRegistry Components = new();

        [DataField, ViewVariables]
        public HashSet<ProtoId<TagPrototype>> Tags = new();

        [DataField, ViewVariables]
        public bool Override = false;

        public FixedPoint2 TileReact(TileRef tile,
            ReagentPrototype reagent,
            FixedPoint2 reactVolume,
            IEntityManager entityManager,
            List<ReagentData>? data)
        {
            if (reactVolume < 5)
                return FixedPoint2.Zero;

            if (entityManager.EntitySysManager.GetEntitySystem<PuddleSystem>()
                .TrySpillAt(tile, new Solution(reagent.ID, reactVolume, data), out var puddleUid, false, false))
            {
                entityManager.AddComponents(puddleUid, Components, Override);
                entityManager.EntitySysManager.GetEntitySystem<TagSystem>().AddTags(puddleUid, Tags);

                return reactVolume;
            }

            return FixedPoint2.Zero;
        }
    }
}
