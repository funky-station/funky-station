// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Atmos;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Cargo.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class GasExtractorConsoleComponent : Component
{
    /// <summary>
    /// List of all currently linked gas extractors.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> LinkedExtractors = new();

    /// <summary>
    /// Multiplier applied to gas purchases made through the console.
    /// </summary>
    [DataField]
    public float PriceMultiplier = 1.0f;
}

[Serializable, NetSerializable]
public sealed class GasExtractorSetSettingsMessage : BoundUserInterfaceMessage
{
    public readonly int ExtractorIndex;
    public readonly float NewSpawnAmount;
    public readonly float NewMaxExternalPressure;

    public GasExtractorSetSettingsMessage(int extractorIndex, float newSpawnAmount, float newMaxExternalPressure)
    {
        ExtractorIndex = extractorIndex;
        NewSpawnAmount = newSpawnAmount;
        NewMaxExternalPressure = newMaxExternalPressure;
    }
}

[Serializable, NetSerializable]
public sealed class BuyMolesForExtractorMessage : BoundUserInterfaceMessage
{
    public int ExtractorIndex { get; }
    public int SpecoAmount { get; }
    public BuyMolesForExtractorMessage(int extractorIndex, int specoAmount)
    {
        ExtractorIndex = extractorIndex;
        SpecoAmount = specoAmount;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleAutoBuyExtractorMessage : BoundUserInterfaceMessage
{
    public int ExtractorIndex { get; }
    public bool Enabled { get; }
    public ToggleAutoBuyExtractorMessage(int extractorIndex, bool enabled)
    {
        ExtractorIndex = extractorIndex;
        Enabled = enabled;
    }
}
