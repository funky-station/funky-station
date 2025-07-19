// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Thaven;

[Serializable, NetSerializable]
public sealed class ThavenMoodsEuiState : EuiStateBase
{
    public List<ThavenMood> Moods { get; }
    public NetEntity Target { get; }
    public ThavenMoodsEuiState(List<ThavenMood> moods, NetEntity target)
    {
        Moods = moods;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class ThavenMoodsSaveMessage : EuiMessageBase
{
    public List<ThavenMood> Moods { get; }
    public NetEntity Target { get; }

    public ThavenMoodsSaveMessage(List<ThavenMood> moods, NetEntity target)
    {
        Moods = moods;
        Target = target;
    }
}
