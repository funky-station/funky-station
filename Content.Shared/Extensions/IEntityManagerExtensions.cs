// SPDX-FileCopyrightText: 2025 duston <66768086+dch-GH@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Extensions;

public static class IEntityManagerExtensions
{
    /// <summary>
    /// Check if an entity is a child of another entity by recursively walking up the transform hierarchy.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="potentialParent">The entity to determine as parent.</param>
    /// <param name="start">The entity we treat as a potential child of the potential parent entity. We start from here.</param>
    /// <param name="maxHeight">How many steps up the child->parent->parent->parent chain you want to try to go. Defaults to 10.</param>
    /// <returns>False if the entity is not a child of the potentialParent or if the entity does not have a TransformComponent (extremely unlikely).</returns>
    public static bool IsChildOf(this IEntityManager self, EntityUid potentialParent, EntityUid start, int maxHeight = 10)
    {
        if (!self.TryGetComponent<TransformComponent>(start, out var transform))
            return false;

        for(var i = 0; i < maxHeight; i++)
        {
            if (transform.ParentUid == potentialParent)
                return true;

            if (!self.TryGetComponent(transform.ParentUid, out transform))
                return false;
        }

        return false;
    }
}
