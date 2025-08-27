// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Content.Shared._Funkystation.Atmos.HFR;

namespace Content.Server._Funkystation.Atmos.Components;

[RegisterComponent]
public sealed partial class HFRCoreComponent : Component
{
    // Connected Parts
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("consoleUid")]
    public EntityUid? ConsoleUid;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("fuelInputUid")]
    public EntityUid? FuelInputUid;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moderatorInputUid")]
    public EntityUid? ModeratorInputUid;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("wasteOutputUid")]
    public EntityUid? WasteOutputUid;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("cornerUids")]
    public List<EntityUid> CornerUids = new();

    // Console Inputs
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isActive")]
    public bool IsActive;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isCooling")]
    public bool IsCooling;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isFuelInjecting")]
    public bool IsFuelInjecting;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isModeratorInjecting")]
    public bool IsModeratorInjecting;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("fuelInputRate")]
    public float FuelInputRate = 25f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moderatorInputRate")]
    public float ModeratorInputRate = 25f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("selectedRecipeId")]
    public string? SelectedRecipeId;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("heatingConductor")]
    public float HeatingConductor = 50f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("coolingVolume")]
    public int CoolingVolume = 50;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("magneticConstrictor")]
    public float MagneticConstrictor = 50f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("currentDamper")]
    public float CurrentDamper = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isWasteRemoving")]
    public bool IsWasteRemoving;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moderatorFilteringRate")]
    public float ModeratorFilteringRate = 25f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("filterGases")]
    public HashSet<Gas> FilterGases = new();

    // State and Control
    [DataField("fusionStarted")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool FusionStarted;

    // Gas Mixtures
    [DataField("internalFusion")]
    [ViewVariables(VVAccess.ReadWrite)]
    public GasMixture? InternalFusion = new GasMixture(volume: 5000f);

    [DataField("moderatorInternal")]
    [ViewVariables(VVAccess.ReadWrite)]
    public GasMixture? ModeratorInternal = new GasMixture(volume: 10000f);

    // Fusion Parameters
    [DataField("energy")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Energy = 0f;

    [DataField("coreTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CoreTemperature = 293.15f;

    [DataField("internalPower")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float InternalPower = 0f;

    [DataField("powerOutput")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PowerOutput = 0f;

    [DataField("instability")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Instability;

    [DataField("deltaTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float DeltaTemperature = 0f;

    [DataField("conduction")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Conduction = 0f;

    [DataField("radiation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Radiation = 0f;

    [DataField("efficiency")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Efficiency = 0f;

    [DataField("heatLimiterModifier")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float HeatLimiterModifier = 0f;

    [DataField("heatOutputMax")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float HeatOutputMax = 0f;

    [DataField("heatOutputMin")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float HeatOutputMin = 0f;

    [DataField("heatOutput")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float HeatOutput = 0f;

    [DataField("powerLevel")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int PowerLevel = 0;

    [DataField("ironContent")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float IronContent = 0f;

    [DataField("areaPower")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float AreaPower = 100f;

    // Integrity and Alerts
    [DataField("criticalThresholdProximity")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CriticalThresholdProximity;

    [DataField("criticalThresholdProximityArchived")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CriticalThresholdProximityArchived;

    [DataField("safeAlert")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string SafeAlert = "Main containment field returning to safe operating parameters.";

    [DataField("warningPoint")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float WarningPoint = 50f;

    [DataField("warningAlert")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string WarningAlert = "Danger! Magnetic containment field faltering!";

    [DataField("emergencyPoint")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float EmergencyPoint = 700f;

    [DataField("emergencyAlert")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string EmergencyAlert = "HYPERTORUS MELTDOWN IMMINENT.";

    [DataField("meltingPoint")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float MeltingPoint = 900f;

    [DataField("hasReachedEmergency")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool HasReachedEmergency;

    [DataField("lastWarning")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastWarning;

    [DataField("lastWarningThresholdProximity")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float LastWarningThresholdProximity;

    [DataField("warningDamageFlags")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int WarningDamageFlags;

    [DataField]
    public HypertorusStatusFlags StatusFlags;

    // GUI Temperatures
    [DataField("fusionTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float FusionTemperature;

    [DataField("fusionTemperatureArchived")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float FusionTemperatureArchived;

    [DataField("moderatorTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float ModeratorTemperature;

    [DataField("moderatorTemperatureArchived")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float ModeratorTemperatureArchived;

    [DataField("coolantTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CoolantTemperature;

    [DataField("coolantTemperatureArchived")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CoolantTemperatureArchived;

    [DataField("outputTemperature")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float OutputTemperature;

    [DataField("outputTemperatureArchived")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float OutputTemperatureArchived;

    [DataField("temperaturePeriod")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float TemperaturePeriod = 1;

    // Meltdown
    [DataField("finalCountdown")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool FinalCountdown;

    [DataField("countdownStartTime")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan CountdownStartTime;

    [DataField("lastCountdownUpdate")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastCountdownUpdate;

    // Sound
    [DataField("lastAccentSound")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastAccentSound;

    [DataField]
    public SoundSpecifier CalmLoopSound = new SoundPathSpecifier("/Audio/_EE/Supermatter/calm.ogg");

    [DataField]
    public SoundSpecifier MeltdownLoopSound = new SoundPathSpecifier("/Audio/_EE/Supermatter/delamming.ogg");

    [DataField]
    public SoundSpecifier? CurrentSoundLoop;

    // Timings
    [DataField("nextGravityPulse")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextGravityPulse;

    [DataField("nextZap")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextZap;

    [DataField("lastTick")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastTick;
}