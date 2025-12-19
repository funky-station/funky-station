// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class HeadToggleSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeadToggleComponent, ToggleHeadEvent>(OnToggleHead);
        SubscribeLocalEvent<HeadToggleComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<HeadToggleComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGetActions(EntityUid uid, HeadToggleComponent component, GetItemActionsEvent args)
    {
        if (_inventorySystem.InSlotWithFlags(uid, SlotFlags.HEAD))
            args.AddAction(ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnToggleHead(Entity<HeadToggleComponent> ent, ref ToggleHeadEvent args)
    {
        var (uid, head) = ent;
        if (head.ToggleActionEntity == null || !_timing.IsFirstTimePredicted || !head.IsEnabled)
            return;

        if (TryComp<InstantActionComponent>(head.ToggleActionEntity, out var action)
            && action.Cooldown.HasValue
            && _timing.CurTime < action.Cooldown.Value.End)
        {
            return;
        }

        if (!_inventorySystem.TryGetSlotEntity(args.Performer, "head", out var existing) || !uid.Equals(existing))
            return;

        bool isActivating;
        if (head.InvertLogic)
            isActivating = !head.IsToggled;
        else
            isActivating = head.IsToggled;

        var soundToPlay = isActivating ? head.SoundToggleOn : head.SoundToggleOff;
        _audio.PlayPredicted(soundToPlay, uid, args.Performer);

        head.IsToggled ^= true;

        var dir = head.IsToggled ? "up" : "down";
        var msg = $"action-head-pull-{dir}-popup-message";
        _popupSystem.PopupClient(Loc.GetString(msg, ("head", uid)), args.Performer, args.Performer);

        var prefix = head.IsToggled ? head.EquippedPrefix : null;
        ToggleHeadComponents(uid, head, args.Performer, prefix);

        if (head.Cooldown.HasValue)
        {
            _actionSystem.SetCooldown(head.ToggleActionEntity.Value, head.Cooldown.Value);
        }
    }

    private void OnGotUnequipped(EntityUid uid, HeadToggleComponent head, GotUnequippedEvent args)
    {
        if (!head.IsToggled || !head.IsEnabled)
            return;

        head.IsToggled = false;
        ToggleHeadComponents(uid, head, args.Equipee, null, true);
    }

    private void ToggleHeadComponents(EntityUid uid, HeadToggleComponent head, EntityUid wearer, string? equippedPrefix = null, bool isEquip = false)
    {
        Dirty(uid, head);
        if (head.ToggleActionEntity is {} action)
            _actionSystem.SetToggled(action, head.IsToggled);

        var headEv = new ItemHeadToggledEvent(wearer, equippedPrefix, head.IsToggled, isEquip);
        RaiseLocalEvent(uid, ref headEv);

        var wearerEv = new WearerHeadToggledEvent(head.IsToggled);
        RaiseLocalEvent(wearer, ref wearerEv);

        var activated = head.InvertLogic ? head.IsToggled : !head.IsToggled;

        var toggledEv = new ItemToggledEvent(Activated: activated, Predicted: false, User: wearer);
        RaiseLocalEvent(uid, ref toggledEv);
    }
}
