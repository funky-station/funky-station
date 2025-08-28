// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// Applies components when effect is activated.
/// </summary>
[RegisterComponent, Access(typeof(XAEApplyComponentsSystem))]
public sealed partial class XAEApplyComponentsComponent : Component
{
    /// <summary>
    /// Components that are permanently added to an entity when the effect's node is entered.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Does adding components need to be done only on first activation.
    /// </summary>
    [DataField]
    public bool ApplyIfAlreadyHave { get; set; }

    /// <summary>
    /// Does component need to be restored when activated 2nd or more times.
    /// </summary>
    [DataField]
    public bool RefreshOnReactivate { get; set; }
}
