using Content.Shared.Tools.Systems;
using Content.Shared._Midnight.Storage;
using Robust.Shared.Serialization;
using Content.Shared.Interaction;
using Content.Shared.Tools;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Radio.Components;

namespace Content.Shared._Midnight.Storage;

public sealed class HiddenStorageSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HiddenStorageComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HiddenStorageComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<HiddenStorageComponent, ToggleHiddenStorageDoAfterEvent>(OnDoAfterComplete);
    }

    private void OnInit(EntityUid uid, HiddenStorageComponent component, ComponentInit args)
    {
        SyncEncryptionState(uid, component.IsOpen);
        UpdateSlotState(uid, component);
    }

    private void OnInteractUsing(EntityUid uid, HiddenStorageComponent component, InteractUsingEvent args)
    {
        if (args.Handled || !_toolSystem.HasQuality(args.Used, component.OpeningTool))
            return;

        args.Handled = true;

        // Start DoAfter
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.OpenDelay,
            new ToggleHiddenStorageDoAfterEvent(), uid, target: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            NeedHand = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;
    }

    private void OnDoAfterComplete(EntityUid uid, HiddenStorageComponent component, ToggleHiddenStorageDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        // Toggle state
        component.IsOpen = !component.IsOpen;
        SyncEncryptionState(uid, component.IsOpen);
        Dirty(uid, component);

        var soundToPlay = component.IsOpen ? component.OpenSound : component.CloseSound;
        _audio.PlayPredicted(soundToPlay, uid, args.User, audioParams: AudioParams.Default.WithMaxDistance(7.5f).WithRolloffFactor(2f));
        
        UpdateSlotState(uid, component);
    }

    private void SyncEncryptionState(EntityUid uid, bool unlocked)
    {
        if (TryComp<EncryptionKeyHolderComponent>(uid, out var keys))
        {
            keys.KeysUnlocked = unlocked;
        }
    }

    private void UpdateSlotState(EntityUid uid, HiddenStorageComponent component)
    {
        if (TryComp<ItemSlotsComponent>(uid, out var itemSlots))
        {
            _itemSlots.SetLock(uid, "holdout", !component.IsOpen, itemSlots);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class ToggleHiddenStorageDoAfterEvent : SimpleDoAfterEvent {}

[Serializable, NetSerializable]
public sealed class HiddenStorageComponentState : ComponentState
{
    public bool IsOpen { get; }

    public HiddenStorageComponentState(bool isOpen)
    {
        IsOpen = isOpen;
    }
}