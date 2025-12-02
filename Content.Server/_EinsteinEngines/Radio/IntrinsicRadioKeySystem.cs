// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Shared.Radio;
using Content.Shared.Radio.Components;

namespace Content.Server._EinsteinEngines.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Server.Radio.Components.IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<Server.Radio.Components.ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, Server.Radio.Components.IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void OnReceiverChannelsChanged(EntityUid uid, Server.Radio.Components.ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void UpdateChannels(EntityUid _, EncryptionKeyHolderComponent keyHolderComp, ref HashSet<string> channels)
    {
        channels.Clear();
        channels.UnionWith(keyHolderComp.Channels);
    }
}
