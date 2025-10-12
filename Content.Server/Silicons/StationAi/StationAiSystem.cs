// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using static Content.Server.Chat.Systems.ChatSystem;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Damage;
using Content.Server.Power.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Destructible;
using Robust.Shared.Containers;

namespace Content.Server.Silicons.StationAi;

public sealed class StationAiSystem : SharedStationAiSystem
{
    [Dependency] private readonly IChatManager _chats = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    private readonly HashSet<Entity<StationAiCoreComponent>> _ais = new();

    /// <summary>
    /// Tracks the last time each AI core was alerted about being under attack to implement cooldown.
    /// </summary>
    private readonly Dictionary<EntityUid, TimeSpan> _attackAlertCooldowns = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandICChatRecipients);
        SubscribeLocalEvent<StationAiCoreComponent, DamageChangedEvent>(OnAiCoreDamaged);
        SubscribeLocalEvent<ApcComponent, ComponentShutdown>(OnApcShutdown);
    }

    private void OnExpandICChatRecipients(ExpandICChatRecipientsEvent ev)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourceXform = Transform(ev.Source);
        var sourcePos = _xforms.GetWorldPosition(sourceXform, xformQuery);

        // This function ensures that chat popups appear on camera views that have connected microphones.
        var query = EntityManager.EntityQueryEnumerator<StationAiCoreComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var entStationAiCore, out var entXform))
        {
            var stationAiCore = new Entity<StationAiCoreComponent?>(ent, entStationAiCore);

            if (!TryGetHeld(stationAiCore, out var insertedAi) || !TryComp(insertedAi, out ActorComponent? actor))
                continue;

            if (stationAiCore.Comp?.RemoteEntity == null || stationAiCore.Comp.Remote)
                continue;

            var xform = Transform(stationAiCore.Comp.RemoteEntity.Value);

            var range = (xform.MapID != sourceXform.MapID)
                ? -1
                : (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();

            if (range < 0 || range > ev.VoiceRange)
                continue;

            ev.Recipients.TryAdd(actor.PlayerSession, new ICChatRecipientData(range, false));
        }
    }

    public override bool SetVisionEnabled(Entity<StationAiVisionComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetVisionEnabled(entity, enabled, announce))
            return false;

        if (announce)
        {
            AnnounceSnip(entity.Owner);
        }

        return true;
    }

    public override bool SetWhitelistEnabled(Entity<StationAiWhitelistComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetWhitelistEnabled(entity, enabled, announce))
            return false;

        if (announce)
        {
            AnnounceSnip(entity.Owner);
        }

        return true;
    }

    public override void AnnounceIntellicardUsage(EntityUid uid, SoundSpecifier? cue = null)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var msg = Loc.GetString("ai-consciousness-download-warning");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chats.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        if (cue != null && _mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindPlaySound(mindId, cue);
    }

    private void AnnounceSnip(EntityUid entity)
    {
        var xform = Transform(entity);

        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        _ais.Clear();
        _lookup.GetChildEntities(xform.GridUid.Value, _ais);
        var filter = Filter.Empty();

        foreach (var ai in _ais)
        {
            // TODO: Filter API?
            if (TryComp(ai.Owner, out ActorComponent? actorComp))
            {
                filter.AddPlayer(actorComp.PlayerSession);
            }
        }

        // TEST
        // filter = Filter.Broadcast();

        // No easy way to do chat notif embeds atm.
        var tile = Maps.LocalToTile(xform.GridUid.Value, grid, xform.Coordinates);
        var msg = Loc.GetString("ai-wire-snipped", ("coords", tile));

        _chats.ChatMessageToMany(ChatChannel.Notifications, msg, msg, entity, false, true, filter.Recipients.Select(o => o.Channel));
        // Apparently there's no sound for this.
    }

    /// <summary>
    /// Funky edit, AI gets alert for damage
    /// </summary>
    private static readonly TimeSpan AttackAlertCooldown = TimeSpan.FromSeconds(10);

    private void OnAiCoreDamaged(EntityUid uid, StationAiCoreComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageDelta.GetTotal() <= 0)
            return;

        var currentTime = _timing.CurTime;
        if (_attackAlertCooldowns.TryGetValue(uid, out var lastAlertTime))
        {
            if (currentTime - lastAlertTime < AttackAlertCooldown)
                return;
        }

        _attackAlertCooldowns[uid] = currentTime;

        // Try to get the AI entity held in this core
        var aiCore = new Entity<StationAiCoreComponent?>(uid, component);
        if (!TryGetHeld(aiCore, out var aiEntity) || !TryComp(aiEntity, out ActorComponent? actor))
            return;

        // Send alert message to the AI player
        var msg = Loc.GetString("ai-core-under-attack");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chats.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        // Play alert sound, could probably make a unique sound for this but for now, default notice noise
        if (_mind.TryGetMind(aiEntity, out var mindId, out _))
        {
            var alertSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");
            _roles.MindPlaySound(mindId, alertSound);
        }
    }

    // Funky edit, malf brain destroyed on APC destruction
    private void OnApcShutdown(EntityUid uid, ApcComponent component, ComponentShutdown args)
    {
        DestroyAiBrainInContainer(uid, StationAiHolderComponent.Container);
    }


    private void DestroyAiBrainInContainer(EntityUid parentEntity, BaseContainer? container)
    {
        if (container == null)
            return;

        foreach (var containedEntity in container.ContainedEntities.ToArray())
        {
            if (HasComp<StationAiHeldComponent>(containedEntity))
            {
                // Make station announcement about AI destruction
                var msg = Loc.GetString("ai-destroyed-announcement");
                _chatSystem.DispatchStationAnnouncement(parentEntity, msg, playDefaultSound: true);

                // Delete the AI brain
                QueueDel(containedEntity);
            }
        }
    }

    private void DestroyAiBrainInContainer(EntityUid parentEntity, string containerName)
    {
        if (!_containers.TryGetContainer(parentEntity, containerName, out var container))
            return;

        DestroyAiBrainInContainer(parentEntity, container);
    }
    // End funky edit
}
