// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;


namespace Content.Shared.FootPrint;

[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    /// <summary>
    ///     Ratio between puddle volume and the amount of reagents that can be transferred from it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 SizeRatio = 0.75f;
}
