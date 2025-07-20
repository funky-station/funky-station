// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.IO;
using System.Linq;
using Content.Shared.Actions;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Shared.ContentPack;
using Robust.Shared.GameStates;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Client.Actions
{
    [UsedImplicitly]
    public sealed class ActionsSystem : SharedActionsSystem
    {
        public delegate void OnActionReplaced(EntityUid actionId);

        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IResourceManager _resources = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        public event Action<EntityUid>? OnActionAdded;
        public event Action<EntityUid>? OnActionRemoved;
        public event Action? ActionsUpdated;
        public event Action<ActionsComponent>? LinkActions;
        public event Action? UnlinkActions;
        public event Action? ClearAssignments;
        public event Action<List<SlotAssignment>>? AssignSlot;

        private readonly List<EntityUid> _removed = new();
        private readonly List<(EntityUid, BaseActionComponent?)> _added = new();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ActionsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ActionsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<ActionsComponent, ComponentHandleState>(HandleComponentState);

            SubscribeLocalEvent<InstantActionComponent, ComponentHandleState>(OnInstantHandleState);
            SubscribeLocalEvent<EntityTargetActionComponent, ComponentHandleState>(OnEntityTargetHandleState);
            SubscribeLocalEvent<WorldTargetActionComponent, ComponentHandleState>(OnWorldTargetHandleState);
            SubscribeLocalEvent<EntityWorldTargetActionComponent, ComponentHandleState>(OnEntityWorldTargetHandleState);
        }

        // goob edit - man fuck them actions bruh
        // (it was breaking our actions system so i smashed it)
        // (regards)

        //public override void FrameUpdate(float frameTime)
        //{
        //    base.FrameUpdate(frameTime);

        //    var worldActionQuery = EntityQueryEnumerator<WorldTargetActionComponent>();
        //    while (worldActionQuery.MoveNext(out var uid, out var action))
        //    {
        //        UpdateAction(uid, action);
        //    }

        //    var instantActionQuery = EntityQueryEnumerator<InstantActionComponent>();
        //    while (instantActionQuery.MoveNext(out var uid, out var action))
        //    {
        //        UpdateAction(uid, action);
        //    }

        //    var entityActionQuery = EntityQueryEnumerator<EntityTargetActionComponent>();
        //    while (entityActionQuery.MoveNext(out var uid, out var action))
        //    {
        //        UpdateAction(uid, action);
        //    }
        //}

        private void OnInstantHandleState(EntityUid uid, InstantActionComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not InstantActionComponentState state)
                return;

            BaseHandleState<InstantActionComponent>(uid, component, state);
        }

        private void OnEntityTargetHandleState(EntityUid uid, EntityTargetActionComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not EntityTargetActionComponentState state)
                return;

            component.Whitelist = state.Whitelist;
            component.Blacklist = state.Blacklist;
            component.CanTargetSelf = state.CanTargetSelf;
            BaseHandleState<EntityTargetActionComponent>(uid, component, state);
        }

        private void OnWorldTargetHandleState(EntityUid uid, WorldTargetActionComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not WorldTargetActionComponentState state)
                return;

            BaseHandleState<WorldTargetActionComponent>(uid, component, state);
        }

        private void OnEntityWorldTargetHandleState(EntityUid uid,
            EntityWorldTargetActionComponent component,
            ref ComponentHandleState args)
        {
            if (args.Current is not EntityWorldTargetActionComponentState state)
                return;

            component.Whitelist = state.Whitelist;
            component.CanTargetSelf = state.CanTargetSelf;
            BaseHandleState<EntityWorldTargetActionComponent>(uid, component, state);
        }

        private void BaseHandleState<T>(EntityUid uid, BaseActionComponent component, BaseActionComponentState state) where T : BaseActionComponent
        {
            // TODO ACTIONS use auto comp states
            component.Icon = state.Icon;
            component.IconOn = state.IconOn;
            component.IconColor = state.IconColor;
            component.OriginalIconColor = state.OriginalIconColor;
            component.DisabledIconColor = state.DisabledIconColor;
            component.Keywords.Clear();
            component.Keywords.UnionWith(state.Keywords);
            component.Enabled = state.Enabled;
            component.Toggled = state.Toggled;
            component.Cooldown = state.Cooldown;
            component.UseDelay = state.UseDelay;
            component.Charges = state.Charges;
            component.MaxCharges = state.MaxCharges;
            component.RenewCharges = state.RenewCharges;
            component.Container = EnsureEntity<T>(state.Container, uid);
            component.EntityIcon = EnsureEntity<T>(state.EntityIcon, uid);
            component.CheckCanInteract = state.CheckCanInteract;
            component.CheckConsciousness = state.CheckConsciousness;
            component.ClientExclusive = state.ClientExclusive;
            component.Priority = state.Priority;
            component.AttachedEntity = EnsureEntity<T>(state.AttachedEntity, uid);
            component.RaiseOnUser = state.RaiseOnUser;
            component.RaiseOnAction = state.RaiseOnAction;
            component.AutoPopulate = state.AutoPopulate;
            component.Temporary = state.Temporary;
            component.ItemIconStyle = state.ItemIconStyle;
            component.Sound = state.Sound;

            UpdateAction(uid, component);
        }

        public override void UpdateAction(EntityUid? actionId, BaseActionComponent? action = null)
        {
            if (!ResolveActionData(actionId, ref action))
                return;

            action.IconColor = action.Charges < 1 ? action.DisabledIconColor : action.OriginalIconColor;

            base.UpdateAction(actionId, action);
            if (_playerManager.LocalEntity != action.AttachedEntity)
                return;

            ActionsUpdated?.Invoke();
        }

        private void HandleComponentState(EntityUid uid, ActionsComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not ActionsComponentState state)
                return;

            _added.Clear();
            _removed.Clear();
            var stateEnts = EnsureEntitySet<ActionsComponent>(state.Actions, uid);
            foreach (var act in component.Actions)
            {
                if (!stateEnts.Contains(act) && !IsClientSide(act))
                    _removed.Add(act);
            }
            component.Actions.ExceptWith(_removed);

            foreach (var actionId in stateEnts)
            {
                if (!actionId.IsValid())
                    continue;

                if (!component.Actions.Add(actionId))
                    continue;

                TryGetActionData(actionId, out var action);
                _added.Add((actionId, action));
            }

            if (_playerManager.LocalEntity != uid)
                return;

            foreach (var action in _removed)
            {
                OnActionRemoved?.Invoke(action);
            }

            _added.Sort(ActionComparer);

            foreach (var action in _added)
            {
                OnActionAdded?.Invoke(action.Item1);
            }

            ActionsUpdated?.Invoke();
        }

        public static int ActionComparer((EntityUid, BaseActionComponent?) a, (EntityUid, BaseActionComponent?) b)
        {
            var priorityA = a.Item2?.Priority ?? 0;
            var priorityB = b.Item2?.Priority ?? 0;
            if (priorityA != priorityB)
                return priorityA - priorityB;

            priorityA = a.Item2?.Container?.Id ?? 0;
            priorityB = b.Item2?.Container?.Id ?? 0;
            return priorityA - priorityB;
        }

        protected override void ActionAdded(EntityUid performer, EntityUid actionId, ActionsComponent comp,
            BaseActionComponent action)
        {
            if (_playerManager.LocalEntity != performer)
                return;

            OnActionAdded?.Invoke(actionId);
        }

        protected override void ActionRemoved(EntityUid performer, EntityUid actionId, ActionsComponent comp, BaseActionComponent action)
        {
            if (_playerManager.LocalEntity != performer)
                return;

            OnActionRemoved?.Invoke(actionId);
        }

        public IEnumerable<(EntityUid Id, BaseActionComponent Comp)> GetClientActions()
        {
            if (_playerManager.LocalEntity is not { } user)
                return Enumerable.Empty<(EntityUid, BaseActionComponent)>();

            return GetActions(user);
        }

        private void OnPlayerAttached(EntityUid uid, ActionsComponent component, LocalPlayerAttachedEvent args)
        {
            LinkAllActions(component);
        }

        private void OnPlayerDetached(EntityUid uid, ActionsComponent component, LocalPlayerDetachedEvent? args = null)
        {
            UnlinkAllActions();
        }

        public void UnlinkAllActions()
        {
            UnlinkActions?.Invoke();
        }

        public void LinkAllActions(ActionsComponent? actions = null)
        {
            if (_playerManager.LocalEntity is not { } user ||
                !Resolve(user, ref actions, false))
            {
                return;
            }

            LinkActions?.Invoke(actions);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            CommandBinds.Unregister<ActionsSystem>();
        }

        public void TriggerAction(EntityUid actionId, BaseActionComponent action)
        {
            if (_playerManager.LocalEntity is not { } user ||
                !TryComp(user, out ActionsComponent? actions))
            {
                return;
            }

            if (action is not InstantActionComponent instantAction)
                return;

            if (action.ClientExclusive)
            {
                PerformAction(user, actions, actionId, instantAction, instantAction.Event, GameTiming.CurTime);
            }
            else
            {
                var request = new RequestPerformActionEvent(GetNetEntity(actionId));
                EntityManager.RaisePredictiveEvent(request);
            }
        }

        /// <summary>
        ///     Load actions and their toolbar assignments from a file.
        /// </summary>
        public void LoadActionAssignments(string path, bool userData)
        {
            if (_playerManager.LocalEntity is not { } user)
                return;

            var file = new ResPath(path).ToRootedPath();
            TextReader reader = userData
                ? _resources.UserData.OpenText(file)
                : _resources.ContentFileReadText(file);

            var yamlStream = new YamlStream();
            yamlStream.Load(reader);

            if (yamlStream.Documents[0].RootNode.ToDataNode() is not SequenceDataNode sequence)
                return;

            ClearAssignments?.Invoke();

            var assignments = new List<SlotAssignment>();

            foreach (var entry in sequence.Sequence)
            {
                if (entry is not MappingDataNode map)
                    continue;

                if (!map.TryGet("action", out var actionNode))
                    continue;

                var action = _serialization.Read<BaseActionComponent>(actionNode, notNullableOverride: true);
                var actionId = Spawn();
                AddComp(actionId, action);
                AddActionDirect(user, actionId);

                if (map.TryGet<ValueDataNode>("name", out var nameNode))
                    _metaData.SetEntityName(actionId, nameNode.Value);

                if (!map.TryGet("assignments", out var assignmentNode))
                    continue;

                var nodeAssignments = _serialization.Read<List<(byte Hotbar, byte Slot)>>(assignmentNode, notNullableOverride: true);

                foreach (var index in nodeAssignments)
                {
                    var assignment = new SlotAssignment(index.Hotbar, index.Slot, actionId);
                    assignments.Add(assignment);
                }
            }

            AssignSlot?.Invoke(assignments);
        }

        public record struct SlotAssignment(byte Hotbar, byte Slot, EntityUid ActionId);
    }
}
