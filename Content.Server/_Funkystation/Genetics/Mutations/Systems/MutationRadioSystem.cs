using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Radio.Components;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationRadioSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutationRadioComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationRadioComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<MutationRadioComponent> ent, ref ComponentInit args)
    {
        var mob = ent.Owner;

        var activeRadio = EnsureComp<ActiveRadioComponent>(mob);
        foreach (var channel in ent.Comp.RadioChannels)
        {
            if (activeRadio.IntrinsicChannels.Add(channel))
                ent.Comp.ActiveAddedChannels.Add(channel);
        }

        EnsureComp<IntrinsicRadioReceiverComponent>(mob);

        var intrinsicRadioTransmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(mob);
        foreach (var channel in ent.Comp.RadioChannels)
        {
            if (intrinsicRadioTransmitter.IntrinsicChannels.Add(channel))
                ent.Comp.TransmitterAddedChannels.Add(channel);
        }
    }

    private void OnShutdown(Entity<MutationRadioComponent> ent, ref ComponentShutdown args)
    {
        var mob = ent.Owner;

        if (TryComp<ActiveRadioComponent>(mob, out var activeRadio))
        {
            foreach (var channel in ent.Comp.ActiveAddedChannels)
            {
                activeRadio.IntrinsicChannels.Remove(channel);
            }
            ent.Comp.ActiveAddedChannels.Clear();

            if (activeRadio.Channels.Count == 0)
                RemCompDeferred<ActiveRadioComponent>(mob);
        }

        if (TryComp<IntrinsicRadioTransmitterComponent>(mob, out var transmitter))
        {
            foreach (var channel in ent.Comp.TransmitterAddedChannels)
            {
                transmitter.IntrinsicChannels.Remove(channel);
            }
            ent.Comp.TransmitterAddedChannels.Clear();

            if (transmitter.IntrinsicChannels.Count == 0)
                RemCompDeferred<IntrinsicRadioTransmitterComponent>(mob);
        }

        if (TryComp<IntrinsicRadioReceiverComponent>(mob, out _) &&
            !HasComp<ActiveRadioComponent>(mob))
        {
            RemCompDeferred<IntrinsicRadioReceiverComponent>(mob);
        }
    }
}
