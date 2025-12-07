// SPDX-FileCopyrightText: 2024 AirFryerBuyOneGetOneFree <jakoblondon01@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.CrewMedal;

[Serializable, NetSerializable]
public enum CrewMedalUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class CrewMedalReasonChangedMessage(string reason) : BoundUserInterfaceMessage
{
    public string Reason { get; } = reason;
}
