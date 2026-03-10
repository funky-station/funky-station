// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Materials;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.ResourceOverview.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResourceOverviewConsoleComponent : Component
{
    /// <summary>
    /// Material prototype IDs that are considered "essential" for low-stock alerts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<MaterialPrototype>> EssentialMaterials = new() { "Steel", "Glass", "Plastic" };

    /// <summary>
    /// Number of sheets below which an entry is considered low stock.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int LowMaterialThreshold = 10;

    /// <summary>
    /// Interval in seconds between UI state updates when the console UI is open.
    /// </summary>
    [DataField]
    public float UpdateTime = 1.0f;

    /// <summary>
    /// Accumulated time since last UI update. Server-only runtime state.
    /// </summary>
    public float UpdateTimer;
}

[Serializable, NetSerializable]
public enum ResourceOverviewConsoleUiKey : byte
{
    Key
}
