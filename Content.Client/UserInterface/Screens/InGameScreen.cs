// SPDX-FileCopyrightText: 2023 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Screens;

/// <summary>
///     Screens that are considered to be 'in-game'.
/// </summary>
public abstract class InGameScreen : UIScreen
{
    public Action<Vector2>? OnChatResized;

    public abstract ChatBox ChatBox { get; }

    public abstract void SetChatSize(Vector2 size);
}
