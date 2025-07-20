// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Radio;

[Serializable, NetSerializable]
public enum IntercomUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ToggleIntercomMicMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleIntercomMicMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleIntercomSpeakerMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleIntercomSpeakerMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public sealed class SelectIntercomChannelMessage : BoundUserInterfaceMessage
{
    public string Channel;

    public SelectIntercomChannelMessage(string channel)
    {
        Channel = channel;
    }
}
