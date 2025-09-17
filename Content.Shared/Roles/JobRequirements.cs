// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 778b <33431126+778b@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Ray <vigersray@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

public static class JobRequirements
{
    /// <summary>
    /// Checks if the requirements of the job are met by the provided play-times.
    /// </summary>
    /// <param name="job"> The job to test. </param>
    /// <param name="playTimes"> The playtimes used for the check. </param>
    /// <param name="reason"> If the requirements were not met, details are provided here. </param>
    /// <returns>Returns true if all requirements were met or there were no requirements.</returns>
    public static bool TryRequirementsMet(
        JobPrototype job,
        IReadOnlyDictionary<string, TimeSpan>? playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile)
    {
        var sys = entManager.System<SharedRoleSystem>();
        var requirements = sys.GetRoleRequirements(job);
        return TryRequirementsMet(requirements, playTimes, out reason, entManager, protoManager, profile);
    }

    /// <summary>
    /// Checks if the list of requirements are met by the provided play-times.
    /// </summary>
    /// <param name="requirements"> The requirements to test. </param>
    /// <param name="playTimes"> The playtimes used for the check. </param>
    /// <param name="reason"> If the requirements were not met, details are provided here. </param>
    /// <returns>Returns true if all requirements were met or there were no requirements.</returns>
    public static bool TryRequirementsMet(
        HashSet<JobRequirement>? requirements,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile)
    {
        reason = null;
        if (requirements == null)
            return true;

        foreach (var requirement in requirements)
        {
            if (!requirement.Check(entManager, protoManager, profile, playTimes, out reason))
                return false;
        }

        return true;
    }

    public static bool TryRequirementsMet(
        ProtoId<JobPrototype> job,
        IReadOnlyDictionary<string, TimeSpan>? playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile)
    {
        if (protoManager.TryIndex(job, out var jobProto))
            return TryRequirementsMet(jobProto, playTimes, out reason, entManager, protoManager, profile);

        reason = FormattedMessage.FromUnformatted("Failed to get job prototype");
        return false;
    }
}

/// <summary>
/// Abstract class for playtime and other requirements for role gates.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public abstract partial class JobRequirement
{
    [DataField]
    public bool Inverted;

    public abstract bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan>? playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason);
}
