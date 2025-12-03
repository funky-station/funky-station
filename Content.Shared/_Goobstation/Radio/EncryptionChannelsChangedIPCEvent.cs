// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Goobstation.Radio.Components;

namespace Content.Shared._Goobstation.Radio;

public sealed class EncryptionChannelsChangedIPCEvent : EntityEventArgs
{
    public readonly EncryptionKeyHolderIPCComponent IpcComponent;

    public EncryptionChannelsChangedIPCEvent(EncryptionKeyHolderIPCComponent ipcComponent)
    {
        IpcComponent = ipcComponent;
    }
}
