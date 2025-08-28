// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Atmos.Components;

[Serializable, NetSerializable]
public sealed class HFRConsoleBoundInterfaceState : BoundUserInterfaceState
{
    public readonly bool IsActive;
    public readonly bool IsCooling;
    public readonly bool IsFuelInjecting;
    public readonly bool IsModeratorInjecting;
    public readonly float FuelInputRate;
    public readonly float ModeratorInputRate;
    public readonly string? SelectedRecipeId;
    public readonly float HeatingConductor;
    public readonly int CoolingVolume;
    public readonly float MagneticConstrictor;
    public readonly float CurrentDamper;
    public readonly bool IsWasteRemoving;
    public readonly float ModeratorFilteringRate;
    public readonly HashSet<Gas> FilterGases;
    public readonly Dictionary<Gas, float> InternalFusionMoles;
    public readonly Dictionary<Gas, float> ModeratorInternalMoles;
    public readonly float CriticalThresholdProximity;
    public readonly float MeltingPoint;
    public readonly float IronContent;
    public readonly float AreaPower;
    public readonly int PowerLevel;
    public readonly float Energy;
    public readonly float Efficiency;
    public readonly float Instability;
    public readonly float FusionTemperature;
    public readonly float FusionTemperatureArchived;
    public readonly float ModeratorTemperature;
    public readonly float ModeratorTemperatureArchived;
    public readonly float CoolantTemperature;
    public readonly float CoolantTemperatureArchived;
    public readonly float OutputTemperature;
    public readonly float OutputTemperatureArchived;
    public readonly float CoolantMoles;
    public readonly float OutputMoles;

    public HFRConsoleBoundInterfaceState(
        bool isActive,
        bool isCooling,
        bool isFuelInjecting,
        bool isModeratorInjecting,
        float fuelInputRate,
        float moderatorInputRate,
        string? selectedRecipeId,
        float heatingConductor,
        int coolingVolume,
        float magneticConstrictor,
        float currentDamper,
        bool isWasteRemoving,
        float moderatorFilteringRate,
        HashSet<Gas> filterGases,
        Dictionary<Gas, float> internalFusionMoles,
        Dictionary<Gas, float> moderatorInternalMoles,
        float criticalThresholdProximity,
        float meltingPoint,
        float ironContent,
        float areaPower,
        int powerLevel,
        float energy,
        float efficiency,
        float instability,
        float fusionTemperature = 3f,
        float fusionTemperatureArchived = 3f,
        float moderatorTemperature = 3f,
        float moderatorTemperatureArchived = 3f,
        float coolantTemperature = 0f,
        float coolantTemperatureArchived = 0f,
        float outputTemperature = 0f,
        float outputTemperatureArchived = 0f,
        float coolantMoles = 0f,
        float outputMoles = 0f)
    {
        IsActive = isActive;
        IsCooling = isCooling;
        IsFuelInjecting = isFuelInjecting;
        IsModeratorInjecting = isModeratorInjecting;
        FuelInputRate = fuelInputRate;
        ModeratorInputRate = moderatorInputRate;
        SelectedRecipeId = selectedRecipeId;
        HeatingConductor = heatingConductor;
        CoolingVolume = coolingVolume;
        MagneticConstrictor = magneticConstrictor;
        CurrentDamper = currentDamper;
        IsWasteRemoving = isWasteRemoving;
        ModeratorFilteringRate = moderatorFilteringRate;
        FilterGases = new HashSet<Gas>(filterGases);
        InternalFusionMoles = new Dictionary<Gas, float>(internalFusionMoles);
        ModeratorInternalMoles = new Dictionary<Gas, float>(moderatorInternalMoles);
        CriticalThresholdProximity = criticalThresholdProximity;
        MeltingPoint = meltingPoint;
        IronContent = ironContent;
        AreaPower = areaPower;
        PowerLevel = powerLevel;
        Energy = energy;
        Efficiency = efficiency;
        Instability = instability;
        FusionTemperature = fusionTemperature;
        FusionTemperatureArchived = fusionTemperatureArchived;
        ModeratorTemperature = moderatorTemperature;
        ModeratorTemperatureArchived = moderatorTemperatureArchived;
        CoolantTemperature = coolantTemperature;
        CoolantTemperatureArchived = coolantTemperatureArchived;
        OutputTemperature = outputTemperature;
        OutputTemperatureArchived = outputTemperatureArchived;
        CoolantMoles = coolantMoles;
        OutputMoles = outputMoles;
    }
}

/// <summary>
/// UI key associated with the atmos monitoring console
/// </summary>
[Serializable, NetSerializable]
public enum HFRConsoleUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class HFRConsoleTogglePowerMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HFRConsoleToggleCoolingMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HFRConsoleToggleFuelInjectionMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HFRConsoleToggleModeratorInjectionMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetFuelInputRateMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetFuelInputRateMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetModeratorInputRateMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetModeratorInputRateMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSelectRecipeMessage : BoundUserInterfaceMessage
{
    public readonly string? RecipeId;
    public HFRConsoleSelectRecipeMessage(string? recipeId) => RecipeId = recipeId;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetHeatingConductorMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetHeatingConductorMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetCoolingVolumeMessage : BoundUserInterfaceMessage
{
    public readonly int Rate;
    public HFRConsoleSetCoolingVolumeMessage(int rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetMagneticConstrictorMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetMagneticConstrictorMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetCurrentDamperMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetCurrentDamperMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleToggleWasteRemoveMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetModeratorFilteringRateMessage : BoundUserInterfaceMessage
{
    public readonly float Rate;
    public HFRConsoleSetModeratorFilteringRateMessage(float rate) => Rate = rate;
}

[Serializable, NetSerializable]
public sealed class HFRConsoleSetFilterGasesMessage : BoundUserInterfaceMessage
{
    public readonly HashSet<Gas> Gases;
    public HFRConsoleSetFilterGasesMessage(HashSet<Gas> gases) => Gases = new HashSet<Gas>(gases);
}

[Serializable, NetSerializable]
public sealed class HFRConsoleUpdateReactorMessage : BoundUserInterfaceMessage
{
    public readonly Dictionary<Gas, float> InternalFusionMoles;
    public readonly Dictionary<Gas, float> ModeratorInternalMoles;
    public readonly float CriticalThresholdProximity;
    public readonly float MeltingPoint;
    public readonly float IronContent;
    public readonly float AreaPower;
    public readonly int PowerLevel;
    public readonly float Energy;
    public readonly float Efficiency;
    public readonly float Instability;
    public readonly string? SelectedRecipeId;
    public readonly float FusionTemperature;
    public readonly float FusionTemperatureArchived;
    public readonly float ModeratorTemperature;
    public readonly float ModeratorTemperatureArchived;
    public readonly float CoolantTemperature;
    public readonly float CoolantTemperatureArchived;
    public readonly float OutputTemperature;
    public readonly float OutputTemperatureArchived;
    public readonly float CoolantMoles;
    public readonly float OutputMoles;

    public HFRConsoleUpdateReactorMessage(
        Dictionary<Gas, float> internalFusionMoles,
        Dictionary<Gas, float> moderatorInternalMoles,
        float criticalThresholdProximity,
        float meltingPoint,
        float ironContent,
        float areaPower,
        int powerLevel,
        float energy,
        float efficiency,
        float instability,
        string? selectedRecipeId,
        float fusionTemperature,
        float fusionTemperatureArchived,
        float moderatorTemperature,
        float moderatorTemperatureArchived,
        float coolantTemperature,
        float coolantTemperatureArchived,
        float outputTemperature,
        float outputTemperatureArchived,
        float coolantMoles,
        float outputMoles)
    {
        InternalFusionMoles = internalFusionMoles;
        ModeratorInternalMoles = moderatorInternalMoles;
        CriticalThresholdProximity = criticalThresholdProximity;
        MeltingPoint = meltingPoint;
        IronContent = ironContent;
        AreaPower = areaPower;
        PowerLevel = powerLevel;
        Energy = energy;
        Efficiency = efficiency;
        Instability = instability;
        SelectedRecipeId = selectedRecipeId;
        FusionTemperature = fusionTemperature;
        FusionTemperatureArchived = fusionTemperatureArchived;
        ModeratorTemperature = moderatorTemperature;
        ModeratorTemperatureArchived = moderatorTemperatureArchived;
        CoolantTemperature = coolantTemperature;
        CoolantTemperatureArchived = coolantTemperatureArchived;
        OutputTemperature = outputTemperature;
        OutputTemperatureArchived = outputTemperatureArchived;
        CoolantMoles = coolantMoles;
        OutputMoles = outputMoles;
    }
}