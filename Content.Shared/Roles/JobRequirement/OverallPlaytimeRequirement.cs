// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class OverallPlaytimeRequirement : JobRequirement
{
    /// <inheritdoc cref="DepartmentTimeRequirement.Time"/>
    [DataField(required: true)]
    public TimeSpan Time;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        var overallTime = playTimes.GetValueOrDefault(PlayTimeTrackingShared.TrackerOverall);
        var overallDiffSpan = Time - overallTime;
        var overallDiff = overallDiffSpan.TotalMinutes;
        var formattedOverallDiff = ContentLocalizationManager.FormatPlaytime(overallDiffSpan);

        if (!Inverted)
        {
            if (overallDiff <= 0 || overallTime >= Time)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-overall-insufficient",
                ("time", formattedOverallDiff)));
            return false;
        }

        if (overallDiff <= 0 || overallTime >= Time)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-overall-too-high",
                ("time", formattedOverallDiff)));
            return false;
        }

        return true;
    }
}
