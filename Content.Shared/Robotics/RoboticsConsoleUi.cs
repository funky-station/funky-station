// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

﻿using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared.Robotics;

[Serializable, NetSerializable]
public enum RoboticsConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class RoboticsConsoleState : BoundUserInterfaceState
{
    /// <summary>
    /// Map of device network addresses to cyborg data.
    /// </summary>
    public Dictionary<string, CyborgControlData> Cyborgs;

    public RoboticsConsoleState(Dictionary<string, CyborgControlData> cyborgs)
    {
        Cyborgs = cyborgs;
    }
}

/// <summary>
/// Message to disable the selected cyborg.
/// </summary>
[Serializable, NetSerializable]
public sealed class RoboticsConsoleDisableMessage : BoundUserInterfaceMessage
{
    public readonly string Address;

    public RoboticsConsoleDisableMessage(string address)
    {
        Address = address;
    }
}

/// <summary>
/// Message to destroy the selected cyborg.
/// </summary>
[Serializable, NetSerializable]
public sealed class RoboticsConsoleDestroyMessage : BoundUserInterfaceMessage
{
    public readonly string Address;

    public RoboticsConsoleDestroyMessage(string address)
    {
        Address = address;
    }
}

/// <summary>
/// Message to impose a Malf-AI Law 0 on the selected cyborg.
/// </summary>
[Serializable, NetSerializable]
public sealed class RoboticsConsoleImposeLawMessage : BoundUserInterfaceMessage
{
    public readonly string Address;

    public RoboticsConsoleImposeLawMessage(string address)
    {
        Address = address;
    }
}

/// <summary>
/// All data a client needs to render the console UI for a single cyborg.
/// Created by <c>BorgTransponderComponent</c> and sent to clients by <c>RoboticsConsoleComponent</c>.
/// </summary>
[DataRecord, Serializable, NetSerializable]
public partial record struct CyborgControlData
{
    /// <summary>
    /// Texture of the borg chassis.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier? ChassisSprite;

    /// <summary>
    /// Name of the borg chassis.
    /// </summary>
    [DataField(required: true)]
    public string ChassisName = string.Empty;

    /// <summary>
    /// Name of the borg's entity, including its silicon id.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// Battery charge from 0 to 1.
    /// </summary>
    [DataField]
    public float Charge;

    /// <summary>
    /// How many modules this borg has, just useful information for roboticists.
    /// Lets them keep track of the latejoin borgs that need new modules and stuff.
    /// </summary>
    [DataField]
    public int ModuleCount;

    /// <summary>
    /// Whether the borg has a brain installed or not.
    /// </summary>
    [DataField]
    public bool HasBrain;

    /// <summary>
    /// Whether the borg can currently be disabled if the brain is installed,
    /// if on cooldown then can't queue up multiple disables.
    /// </summary>
    [DataField]
    public bool CanDisable;

    /// <summary>
    /// Whether the borg is emagged for interaction immunity.
    /// Used to grey out the impose-law button on the console UI.
    /// </summary>
    [DataField]
    public bool Emagged;


    /// <summary>
    /// When this cyborg's data will be deleted.
    /// Set by the console when receiving the packet.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Timeout = TimeSpan.Zero;

    public CyborgControlData(SpriteSpecifier? chassisSprite, string chassisName, string name, float charge, int moduleCount, bool hasBrain, bool canDisable, bool emagged)
    {
        ChassisSprite = chassisSprite;
        ChassisName = chassisName;
        Name = name;
        Charge = charge;
        ModuleCount = moduleCount;
        HasBrain = hasBrain;
        CanDisable = canDisable;
        Emagged = emagged;
    }
}

public static class RoboticsConsoleConstants
{
    // broadcast by cyborgs on Robotics Console frequency
    public const string NET_CYBORG_DATA = "cyborg-data";

    // sent by robotics console to cyborgs on Cyborg Control frequency
    public const string NET_DISABLE_COMMAND = "cyborg-disable";
    public const string NET_DESTROY_COMMAND = "cyborg-destroy";

    // Malf AI: instruct cyborg to add Law 0 (obey AI).
    public const string NET_IMPOSE_LAW0_COMMAND = "cyborg-law0";

    // Additional payload key: the AI entity that imposed Law 0
    public const string NET_IMPOSED_BY = "cyborg-law0-by";
}
