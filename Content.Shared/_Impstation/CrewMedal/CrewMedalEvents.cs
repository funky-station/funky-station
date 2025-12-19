// SPDX-FileCopyrightText: 2025 AirFryerBuyOneGetOneFree <jakoblondon01@gmail.com>
// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

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

[Serializable, NetSerializable]
public sealed partial class CrewMedalAwardDoAfterEvent : SimpleDoAfterEvent
{
}
