// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Radio;
using Content.Shared._Goobstation.Radio.Components;

namespace Content.Server._EinsteinEngines.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterIPCComponent, EncryptionChannelsChangedIPCEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioIPCComponent, EncryptionChannelsChangedIPCEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, Shared._Goobstation.Radio.Components.IntrinsicRadioTransmitterIPCComponent ipcComponent, Shared._Goobstation.Radio.EncryptionChannelsChangedIPCEvent args)
    {
        UpdateChannels(uid, args.IpcComponent, ref ipcComponent.Channels);
    }

    private void OnReceiverChannelsChanged(EntityUid uid, Shared._Goobstation.Radio.Components.ActiveRadioIPCComponent ipcComponent, Shared._Goobstation.Radio.EncryptionChannelsChangedIPCEvent args)
    {
        UpdateChannels(uid, args.IpcComponent, ref ipcComponent.Channels);
    }

    private void UpdateChannels(EntityUid _, Shared._Goobstation.Radio.Components.EncryptionKeyHolderIPCComponent keyHolderIpcComp, ref HashSet<string> channels)
    {
        channels.Clear();
        channels.UnionWith(keyHolderIpcComp.Channels);
    }
}
