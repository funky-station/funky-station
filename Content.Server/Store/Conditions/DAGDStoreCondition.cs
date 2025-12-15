// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Store;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Filters out listings with DAGDOnly tag from stores that don't have the DAGDStore tag.
/// </summary>
public sealed partial class DAGDStoreCondition : ListingCondition
{
    private static readonly ProtoId<TagPrototype> DAGDOnlyTag = "DAGDOnly";
    private static readonly ProtoId<TagPrototype> DAGDStoreTag = "DAGDStore";

    public override bool Condition(ListingConditionArgs args)
    {
        // If listing doesn't have DAGDOnly tag, show it in all stores
        if (!args.Listing.Tags.Contains(DAGDOnlyTag))
            return true;

        // If listing has DAGDOnly tag, only show it in stores with DAGDStore tag
        if (args.StoreEntity == null)
            return false;

        var tagSystem = args.EntityManager.System<TagSystem>();
        return tagSystem.HasTag(args.StoreEntity.Value, DAGDStoreTag);
    }
}

