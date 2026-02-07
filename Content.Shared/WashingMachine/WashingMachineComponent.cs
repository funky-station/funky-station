// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 MaiaArai <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.WashingMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class WashingMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan WashingTime;

    [ViewVariables, AutoNetworkedField, AutoPausedField, Access(typeof(SharedWashingMachineSystem))]
    public TimeSpan WashingFinished;

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(6);

    [ViewVariables, AutoNetworkedField, AutoPausedField, Access(typeof(SharedWashingMachineSystem))]
    public TimeSpan NextWashAllowed;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? WashingSound;

    public EntityUid? WashingSoundStream;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? FinishedSound;

    [ViewVariables, AutoNetworkedField, Access(typeof(SharedWashingMachineSystem))]
    public WashingMachineState WashingMachineState;

    [DataField, AutoNetworkedField]
    public float BluntDamagePerSecond = 6.0f;

    [DataField, AutoNetworkedField]
    public float ThumpSoundChance = 0.9f;

    [DataField, AutoNetworkedField]
    public string WaterSprayReagent = "Water";

    [DataField, AutoNetworkedField]
    public float WaterSprayAmount = 150.0f;

    [DataField, AutoNetworkedField]
    public float WaterSprayChance = 1.0f;

    [DataField, AutoNetworkedField]
    public float MigraineDuration = 30.0f;

    [DataField, AutoNetworkedField]
    public float MigraineMagnitude = 5.0f;

    [DataField, AutoNetworkedField]
    public float SelfDamagePerSecond = 6.0f;

    [ViewVariables, AutoNetworkedField, Access(typeof(SharedWashingMachineSystem))]
    public float AccumulatedSelfDamage = 0f;
}
