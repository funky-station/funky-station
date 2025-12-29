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
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.IO;
using System.Linq;
using Content.Client._RMC14.Movement;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Mapping;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Shared.ContentPack;
using Robust.Shared.GameStates;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Client.Actions
{
    [UsedImplicitly]
    public sealed class ActionsSystem : SharedActionsSystem
    {
        public delegate void OnActionReplaced(EntityUid actionId);

        [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
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
        private readonly List<Entity<ActionComponent>> _added = new();

        public static readonly EntProtoId MappingEntityAction = "BaseMappingEntityAction";

        // RMC14
        [Dependency] private readonly RMCLagCompensationSystem _rmcLagCompensation = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ActionsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ActionsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<ActionsComponent, ComponentHandleState>(OnHandleState);

            SubscribeLocalEvent<ActionComponent, AfterAutoHandleStateEvent>(OnActionAutoHandleState);

            SubscribeLocalEvent<EntityTargetActionComponent, ActionTargetAttemptEvent>(OnEntityTargetAttempt);
            SubscribeLocalEvent<WorldTargetActionComponent, ActionTargetAttemptEvent>(OnWorldTargetAttempt);
        }


        private void OnActionAutoHandleState(Entity<ActionComponent> ent, ref AfterAutoHandleStateEvent args)
        {
            UpdateAction(ent);
        }

        public override void UpdateAction(Entity<ActionComponent> ent)
        {
            // TODO: Decouple this.
            ent.Comp.IconColor = _sharedCharges.GetCurrentCharges(ent.Owner) == 0 ? ent.Comp.DisabledIconColor : ent.Comp.OriginalIconColor;
            base.UpdateAction(ent);
            if (_playerManager.LocalEntity != ent.Comp.AttachedEntity)
                return;

            ActionsUpdated?.Invoke();
        }

        private void OnHandleState(Entity<ActionsComponent> ent, ref ComponentHandleState args)
        {
            if (args.Current is not ActionsComponentState state)
                return;

            var (uid, comp) = ent;
            _added.Clear();
            _removed.Clear();
            var stateEnts = EnsureEntitySet<ActionsComponent>(state.Actions, uid);
            foreach (var act in comp.Actions)
            {
                if (!stateEnts.Contains(act) && !IsClientSide(act))
                    _removed.Add(act);
            }
            comp.Actions.ExceptWith(_removed);

            foreach (var actionId in stateEnts)
            {
                if (!actionId.IsValid())
                    continue;

                if (!comp.Actions.Add(actionId))
                    continue;

                if (GetAction(actionId) is {} action)
                    _added.Add(action);
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
                OnActionAdded?.Invoke(action);
            }

            ActionsUpdated?.Invoke();
        }

        public static int ActionComparer(Entity<ActionComponent> a, Entity<ActionComponent> b)
        {
            var priorityA = a.Comp?.Priority ?? 0;
            var priorityB = b.Comp?.Priority ?? 0;
            if (priorityA != priorityB)
                return priorityA - priorityB;

            priorityA = a.Comp?.Container?.Id ?? 0;
            priorityB = b.Comp?.Container?.Id ?? 0;
            return priorityA - priorityB;
        }

        protected override void ActionAdded(Entity<ActionsComponent> performer, Entity<ActionComponent> action)
        {
            if (_playerManager.LocalEntity != performer.Owner)
                return;

            OnActionAdded?.Invoke(action);
            ActionsUpdated?.Invoke();
        }

        protected override void ActionRemoved(Entity<ActionsComponent> performer, Entity<ActionComponent> action)
        {
            if (_playerManager.LocalEntity != performer.Owner)
                return;

            OnActionRemoved?.Invoke(action);
            ActionsUpdated?.Invoke();
        }

        public IEnumerable<Entity<ActionComponent>> GetClientActions()
        {
            if (_playerManager.LocalEntity is not { } user)
                return Enumerable.Empty<Entity<ActionComponent>>();

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

        public void TriggerAction(Entity<ActionComponent> action)
        {
            if (_playerManager.LocalEntity is not { } user)
                return;

            // TODO: unhardcode this somehow

            if (!HasComp<InstantActionComponent>(action))
                return;

            if (action.Comp.ClientExclusive)
            {
                PerformAction(user, action);
            }
            else
            {
                var request = new RequestPerformActionEvent(GetNetEntity(action), _rmcLagCompensation.GetLastRealTick(null));
                RaisePredictiveEvent(request);
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

            var actions = EnsureComp<ActionsComponent>(user);

            ClearAssignments?.Invoke();

            var assignments = new List<SlotAssignment>();
            foreach (var entry in sequence.Sequence)
            {
                if (entry is not MappingDataNode map)
                    continue;

                if (!map.TryGet("assignments", out var assignmentNode))
                    continue;

                var actionId = EntityUid.Invalid;
                if (map.TryGet<ValueDataNode>("action", out var actionNode))
                {
                    var id = new EntProtoId(actionNode.Value);
                    actionId = Spawn(id);
                }
                else if (map.TryGet<ValueDataNode>("entity", out var entityNode))
                {
                    var id = new EntProtoId(entityNode.Value);
                    var proto = _proto.Index(id);
                    actionId = Spawn(MappingEntityAction);
                    SetIcon(actionId, new SpriteSpecifier.EntityPrototype(id));
                    SetEvent(actionId, new StartPlacementActionEvent()
                    {
                        PlacementOption = "SnapgridCenter",
                        EntityType = id
                    });
                    _metaData.SetEntityName(actionId, proto.Name);
                }
                else if (map.TryGet<ValueDataNode>("tileId", out var tileNode))
                {
                    var id = new ProtoId<ContentTileDefinition>(tileNode.Value);
                    var proto = _proto.Index(id);
                    actionId = Spawn(MappingEntityAction);
                    if (proto.Sprite is {} sprite)
                        SetIcon(actionId, new SpriteSpecifier.Texture(sprite));
                    SetEvent(actionId, new StartPlacementActionEvent()
                    {
                        PlacementOption = "AlignTileAny",
                        TileId = id
                    });
                    _metaData.SetEntityName(actionId, Loc.GetString(proto.Name));
                }
                else
                {
                    Log.Error($"Mapping actions from {path} had unknown action data!");
                    continue;
                }

                AddActionDirect((user, actions), actionId);
            }
        }

        private void OnWorldTargetAttempt(Entity<WorldTargetActionComponent> ent, ref ActionTargetAttemptEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;

            var (uid, comp) = ent;
            var action = args.Action;
            var coords = args.Input.Coordinates;
            var user = args.User;

            if (!ValidateWorldTarget(user, coords, ent))
                return;

            // optionally send the clicked entity too, if it matches its whitelist etc
            // this is the actual entity-world targeting magic
            EntityUid? targetEnt = null;
            if (TryComp<EntityTargetActionComponent>(ent, out var entity) &&
                args.Input.EntityUid != null &&
                ValidateEntityTarget(user, args.Input.EntityUid, (uid, entity)))
            {
                targetEnt = args.Input.EntityUid;
            }

            if (action.ClientExclusive)
            {
                // TODO: abstract away from single event or maybe just RaiseLocalEvent?
                if (comp.Event is {} ev)
                {
                    ev.Target = coords;
                    ev.Entity = targetEnt;
                }

                PerformAction((user, user.Comp), (uid, action));
            }
            else
                RaisePredictiveEvent(new RequestPerformActionEvent(GetNetEntity(uid), GetNetEntity(targetEnt), GetNetCoordinates(coords), _rmcLagCompensation.GetLastRealTick(null)));

            args.FoundTarget = true;
        }

        private void OnEntityTargetAttempt(Entity<EntityTargetActionComponent> ent, ref ActionTargetAttemptEvent args)
        {
            if (args.Handled || args.Input.EntityUid is not { Valid: true } entity)
                return;

            // let world target component handle it
            var (uid, comp) = ent;
            if (comp.Event is not {} ev)
            {
                DebugTools.Assert(HasComp<WorldTargetActionComponent>(ent), $"Action {ToPrettyString(ent)} requires WorldTargetActionComponent for entity-world targeting");
                return;
            }

            args.Handled = true;

            var action = args.Action;
            var user = args.User;

            if (!ValidateEntityTarget(user, entity, ent))
                return;

            if (action.ClientExclusive)
            {
                ev.Target = entity;

                PerformAction((user, user.Comp), (uid, action));
            }
            else
            {
                RaisePredictiveEvent(new RequestPerformActionEvent(GetNetEntity(uid), GetNetEntity(entity), _rmcLagCompensation.GetLastRealTick(null)));
            }

            args.FoundTarget = true;
        }

        public record struct SlotAssignment(byte Hotbar, byte Slot, EntityUid ActionId);
    }
}
