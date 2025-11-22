// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LaryNevesPR <LaryNevesPR@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Barks;

[Serializable, NetSerializable]
public sealed class PlayBarkEvent(NetEntity sourceUid, string message, bool whisper) : EntityEventArgs
{
    public NetEntity SourceUid { get; } = sourceUid;
    public string Message { get; } = message;
    public bool Whisper { get; } = whisper;
}

[Serializable, NetSerializable]
public sealed class PreviewBarkEvent(string barkProtoID) : EntityEventArgs
{
    public string BarkProtoID { get; } = barkProtoID;
}
