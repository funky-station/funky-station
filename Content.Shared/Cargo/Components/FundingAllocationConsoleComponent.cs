// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// A console that manipulates the distribution of revenue on the station.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedCargoSystem))]
public sealed partial class FundingAllocationConsoleComponent : Component
{
    /// <summary>
    /// Sound played when the budget distribution is set.
    /// </summary>
    [DataField]
    public SoundSpecifier SetDistributionSound = new SoundCollectionSpecifier("CargoPing");
}

[Serializable, NetSerializable]
public sealed class SetFundingAllocationBuiMessage : BoundUserInterfaceMessage
{
    public Dictionary<ProtoId<CargoAccountPrototype>, int> Percents;

    public SetFundingAllocationBuiMessage(Dictionary<ProtoId<CargoAccountPrototype>, int> percents)
    {
        Percents = percents;
    }
}

[Serializable, NetSerializable]
public sealed class FundingAllocationConsoleBuiState : BoundUserInterfaceState
{
    public NetEntity Station;

    public FundingAllocationConsoleBuiState(NetEntity station)
    {
        Station = station;
    }
}

[Serializable, NetSerializable]
public enum FundingAllocationConsoleUiKey : byte
{
    Key
}
