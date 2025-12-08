// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
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
    public ZombieTumorInfectionStage Stage = ZombieTumorInfectionStage.Incubation;

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
    /// Next time to show a random sickness message
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextSicknessMessage;

    /// <summary>
    /// Next time to cough
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextCough;

    /// <summary>
    /// Time when the entity should receive the zombify self ability
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? ZombifySelfAbilityTime;

    /// <summary>
    /// Time when the entity should be auto-zombified
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? AutoZombifyTime;

    /// <summary>
    /// Damage dealt per tick in early stage before tumor formation
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier EarlyDamage = new()
    {
        DamageDict = new()
        {
            { "Poison", 0.1 }
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
            { "Poison", 0.2 }
        }
    };

    /// <summary>
    /// Damage dealt per tick in advanced stage
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier AdvancedDamage = new()
    {
        DamageDict = new()
        {
            { "Poison", 0.5 }
        }
    };

    /// <summary>
    /// Multiplier for damage when entity is in critical condition.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CritDamageMultiplier = 10f;

    /// <summary>
    /// Time to progress from incubation to early stage (when symptoms begin).
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan IncubationToEarlyTime = TimeSpan.FromMinutes(10); //final version
    //public TimeSpan IncubationToEarlyTime = TimeSpan.FromSeconds(5); //test version

    /// <summary>
    /// Time to progress from early to tumor formation stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EarlyToTumorTime = TimeSpan.FromMinutes(10); //final version
    //public TimeSpan EarlyToTumorTime = TimeSpan.FromSeconds(10); //test version

    /// <summary>
    /// Time to progress from tumor to advanced stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TumorToAdvancedTime = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Amount of oil to drain per tick for IPCs in Early stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EarlyOilDrain = 0.1f;

    /// <summary>
    /// Amount of oil to drain per tick for IPCs in TumorFormed stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TumorOilDrain = 0.2f;

    /// <summary>
    /// Amount of oil to drain per tick for IPCs in Advanced stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AdvancedOilDrain = 0.5f;

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
    /// Incubation period - no symptoms or damage, body is unchanged.
    /// </summary>
    Incubation = 0,

    /// <summary>
    /// Early infection - symptoms begin, poison damage starts.
    /// </summary>
    Early = 1,

    /// <summary>
    /// Tumor has formed inside the body.
    /// </summary>
    TumorFormed = 2,

    /// <summary>
    /// Advanced infection - increased damage.
    /// </summary>
    Advanced = 3
}
