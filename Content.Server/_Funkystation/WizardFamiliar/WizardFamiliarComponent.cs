// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.WizardFamiliar;

/// <summary>
/// Marks an entity as a wizard's familiar. Stores the wizard who summoned them.
/// </summary>
[RegisterComponent]
public sealed partial class WizardFamiliarComponent : Component
{
    /// <summary>
    /// The wizard who summoned this familiar.
    /// </summary>
    [DataField]
    public EntityUid? Wizard;
}
