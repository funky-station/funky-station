// SPDX-FileCopyrightText: 2024 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared._EstacaoPirata.Cards.Hand;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class CardHandComponent : Component
{
    [DataField("angle")]
    public float Angle = 120f;

    [DataField("xOffset")]
    public float XOffset = 0.5f;

    [DataField("scale")]
    public float Scale = 1;

    [DataField("limit")]
    public int CardLimit = 10;

    [DataField("flipped")]
    public bool Flipped = false;
}


[Serializable, NetSerializable]
public enum CardUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class CardHandDrawMessage(NetEntity card) : BoundUserInterfaceMessage
{
    public NetEntity Card = card;
}
