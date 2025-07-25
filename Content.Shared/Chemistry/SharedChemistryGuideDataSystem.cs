// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry;

/// <summary>
/// This handles the chemistry guidebook and caching it.
/// </summary>
public abstract class SharedChemistryGuideDataSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;

    protected readonly Dictionary<string, ReagentGuideEntry> Registry = new();

    public IReadOnlyDictionary<string, ReagentGuideEntry> ReagentGuideRegistry => Registry;

    // Only ran on the server
    public abstract void ReloadAllReagentPrototypes();
}

[Serializable, NetSerializable]
public sealed class ReagentGuideRegistryChangedEvent : EntityEventArgs
{
    public ReagentGuideChangeset Changeset;

    public ReagentGuideRegistryChangedEvent(ReagentGuideChangeset changeset)
    {
        Changeset = changeset;
    }
}

[Serializable, NetSerializable]
public sealed class ReagentGuideChangeset
{
    public Dictionary<string,ReagentGuideEntry> GuideEntries;

    public HashSet<string> Removed;

    public ReagentGuideChangeset(Dictionary<string, ReagentGuideEntry> guideEntries, HashSet<string> removed)
    {
        GuideEntries = guideEntries;
        Removed = removed;
    }
}
