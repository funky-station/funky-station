// SPDX-FileCopyrightText: 2025 Sophia Rustfield <gitlab@catwolf.xyz>
// SPDX-FileCopyrightText: 2025 SpaceCat~Chan <49094338+SpaceCat-Chan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.FixedPoint;

[RegisterComponent]
public sealed partial class LeakyObjectComponent : Component
{
    [DataField]
    public string SolutionName = "leaky";
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(0.5);
    [DataField]
    public float LeakEfficiency = 0.5f;
    [DataField]
    /// <summary>
    /// how often the object should transfer sulution
    /// </summary>
    public float UpdateTime = 4f;
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
    [DataField("minVelocity")]
    public float MinimumSpillVelocity = 3f;
    [DataField]
    public FixedPoint2 SpillAmount = FixedPoint2.New(2);
}
