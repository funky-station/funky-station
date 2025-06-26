using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Radio.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Radio.EntitySystems;

public sealed class HeadsetSystem : SharedHeadsetSystem
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly RadioSystem _radio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeadsetComponent, RadioReceiveEvent>(OnHeadsetReceive);
        SubscribeLocalEvent<HeadsetComponent, EncryptionChannelsChangedEvent>(OnKeysChanged);

        SubscribeLocalEvent<WearingHeadsetComponent, EntitySpokeEvent>(OnSpeak);

        SubscribeLocalEvent<HeadsetComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnKeysChanged(EntityUid uid, HeadsetComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateRadioChannels(uid, component, args.Component);
    }

    private void UpdateRadioChannels(EntityUid uid, HeadsetComponent headset, EncryptionKeyHolderComponent? keyHolder = null)
    {
        // make sure to not add ActiveRadioComponent when headset is being deleted
        if (!headset.Enabled || MetaData(uid).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        if (!Resolve(uid, ref keyHolder))
            return;

        if (keyHolder.Channels.Count == 0)
            RemComp<ActiveRadioComponent>(uid);
        else
            EnsureComp<ActiveRadioComponent>(uid).Channels = new(keyHolder.Channels);
    }

    private void OnSpeak(EntityUid uid, WearingHeadsetComponent component, EntitySpokeEvent args)
    {
        // Midnight - Handle multiple headsets by checking all equipped ones
        if (args.Channel != null)
        {
            var headsetUsed = false;
            
            // Check all headsets this entity is wearing
            foreach (var headset in component.Headsets)
            {
                if (TryComp(headset, out EncryptionKeyHolderComponent? keys)
                    && keys.Channels.Contains(args.Channel.ID))
                {
                    _radio.SendRadioMessage(uid, args.Message, args.Channel, headset);
                    headsetUsed = true;
                    break; // Only use the first available headset to prevent duplicates
                }
            }
            
            if (headsetUsed)
                args.Channel = null;
        }
    }

    protected override void OnGotEquipped(EntityUid uid, HeadsetComponent component, GotEquippedEvent args)
    {
        base.OnGotEquipped(uid, component, args);
        if (component.Enabled)
        {
            component.IsEquipped = true;
            var wearingComponent = EnsureComp<WearingHeadsetComponent>(args.Equipee);
            wearingComponent.Headsets.Add(uid);
            UpdateRadioChannels(uid, component);
        }
    }

    protected override void OnGotUnequipped(EntityUid uid, HeadsetComponent component, GotUnequippedEvent args)
    {
        base.OnGotUnequipped(uid, component, args);
        component.IsEquipped = false;
        RemComp<ActiveRadioComponent>(uid);
        
        // Remove this headset from the wearing component
        if (TryComp<WearingHeadsetComponent>(args.Equipee, out var wearing))
        {
            wearing.Headsets.Remove(uid);
            
            // If no headsets left, remove the component
            if (wearing.Headsets.Count == 0)
                RemComp<WearingHeadsetComponent>(args.Equipee);
        }
    }

    public void SetEnabled(EntityUid uid, bool value, HeadsetComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Enabled == value)
            return;

        if (!value)
        {
            RemCompDeferred<ActiveRadioComponent>(uid);

            if (component.IsEquipped)
            {
                var parentUid = Transform(uid).ParentUid;
                if (TryComp<WearingHeadsetComponent>(parentUid, out var wearing))
                {
                    wearing.Headsets.Remove(uid);
                    if (wearing.Headsets.Count == 0)
                        RemCompDeferred<WearingHeadsetComponent>(parentUid);
                }
            }
        }
        else if (component.IsEquipped)
        {
            var parentUid = Transform(uid).ParentUid;
            EnsureComp<WearingHeadsetComponent>(parentUid).Headsets.Add(uid);
            UpdateRadioChannels(uid, component);
        }
    }

    private void OnHeadsetReceive(EntityUid uid, HeadsetComponent component, ref RadioReceiveEvent args)
    {
        if (TryComp(Transform(uid).ParentUid, out ActorComponent? actor))
            _netMan.ServerSendMessage(args.ChatMsg, actor.PlayerSession.Channel);
    }

    private void OnEmpPulse(EntityUid uid, HeadsetComponent component, ref EmpPulseEvent args)
    {
        if (component.Enabled)
        {
            args.Affected = true;
            args.Disabled = true;
        }
    }
}
