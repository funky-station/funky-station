// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Zombies;

/// <summary>
/// Component tracking zombie tumor infection progression.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedZombieTumorOrganSystem))]
public sealed partial class ZombieTumorInfectionComponent : Component
{
    /// <summary>
    /// Current stage of the infection.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ZombieTumorInfectionStage Stage = ZombieTumorInfectionStage.Early;

    /// <summary>
    /// Time when the infection will progress to the next stage.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextStageAt;

    /// <summary>
    /// Next time to apply damage/effects.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextTick;

    /// <summary>
    /// Damage dealt per tick in early stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier EarlyDamage = new()
    {
        DamageDict = new()
        {
            { "Poison", 0.2 }
        }
    };

    /// <summary>
    /// Damage dealt per tick after tumor formation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier TumorDamage = new()
    {
        DamageDict = new()
        {
            { "Poison", 0.5 }
        }
    };

    /// <summary>
    /// Damage dealt per tick in advanced stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier AdvancedDamage = new()
    {
        DamageDict = new()
        {
            { "Poison", 1.0 }
        }
    };

    /// <summary>
    /// Multiplier for damage when entity is in critical condition.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CritDamageMultiplier = 10f;

    /// <summary>
    /// Time to progress from early to tumor formation stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EarlyToTumorTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time to progress from tumor to advanced stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TumorToAdvancedTime = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Amount of oil to drain per tick for IPCs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float OilDrainAmount = 0.5f;

    /// <summary>
    /// Radiation damage per tick when IPC oil is empty.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier RadiationDamage = new()
    {
        DamageDict = new()
        {
            { "Radiation", 1.0 }
        }
    };
}

/// <summary>
/// Stages of zombie tumor infection progression.
/// </summary>
public enum ZombieTumorInfectionStage : byte
{
    /// <summary>
    /// Early infection - just poison damage.
    /// </summary>
    Early = 0,

    /// <summary>
    /// Tumor has formed inside the body.
    /// </summary>
    TumorFormed = 1,

    /// <summary>
    /// Advanced infection - increased damage.
    /// </summary>
    Advanced = 2
}
