// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 moneyl <8206401+Moneyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 DTanxxx <55208219+DTanxxx@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 R. Neuser <rneuser@iastate.edu>
// SPDX-FileCopyrightText: 2020 Tad Hardesty <tad@platymuus.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Daniel Castro Razo <eldanielcr@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.Popups;
using Content.Shared.CCVar;
using Content.Shared.Examine;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Client.ComponentTrees;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Verbs
{
    [UsedImplicitly]
    public sealed class VerbSystem : SharedVerbSystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly ExamineSystem _examine = default!;
        [Dependency] private readonly SpriteTreeSystem _tree = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly IStateManager _stateManager = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly SharedContainerSystem _containers = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;

        private float _lookupSize;

        private static readonly ProtoId<TagPrototype> HideContextMenuTag = "HideContextMenu";

        /// <summary>
        ///     These flags determine what entities the user can see on the context menu.
        /// </summary>
        public MenuVisibility Visibility;

        public Action<VerbsResponseEvent>? OnVerbsResponse;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<VerbsResponseEvent>(HandleVerbResponse);
            Subs.CVar(_cfg, CCVars.GameEntityMenuLookup, OnLookupChanged, true);
        }

        private void OnLookupChanged(float val)
        {
            _lookupSize = val;
        }

        /// <summary>
        /// Get all of the entities in an area for displaying on the context menu.
        /// </summary>
        /// <returns>True if any entities were found.</returns>
        public bool TryGetEntityMenuEntities(MapCoordinates targetPos, [NotNullWhen(true)] out List<EntityUid>? entities)
        {
            entities = null;

            if (_stateManager.CurrentState is not GameplayStateBase)
                return false;

            if (_playerManager.LocalEntity is not { } player)
                return false;

            // If FOV drawing is disabled, we will modify the visibility option to ignore visiblity checks.
            var visibility = _eyeManager.CurrentEye.DrawFov ? Visibility : Visibility | MenuVisibility.NoFov;

            var ev = new MenuVisibilityEvent
            {
                TargetPos = targetPos,
                Visibility = visibility,
            };

            RaiseLocalEvent(player, ref ev);
            visibility = ev.Visibility;

            // Initially, we include all entities returned by a sprite area lookup
            var box = Box2.CenteredAround(targetPos.Position, new Vector2(_lookupSize, _lookupSize));
            var queryResult = _tree.QueryAabb(targetPos.MapId, box);
            entities = new List<EntityUid>(queryResult.Count);
            foreach (var ent in queryResult)
            {
                entities.Add(ent.Uid);
            }

            // If we're in a container list all other entities in it.
            // E.g., allow players in lockers to examine / interact with other entities in the same locker
            if (_containers.TryGetContainingContainer((player, null), out var container))
            {
                // Only include the container contents when clicking near it.
                if (entities.Contains(container.Owner)
                    || _containers.TryGetOuterContainer(container.Owner, Transform(container.Owner), out var outer)
                    && entities.Contains(outer.Owner))
                {
                    // The container itself might be in some other container, so it might not have been added by the
                    // sprite tree lookup.
                    if (!entities.Contains(container.Owner))
                        entities.Add(container.Owner);

                    // TODO Context Menu
                    // This might miss entities in some situations. E.g., one of the contained entities entity in it, that
                    // itself has another entity attached to it, then we should be able to "see" that entity.
                    // E.g., if a security guard is on a segway and gets thrown in a locker, this wouldn't let you see the guard.
                    foreach (var ent in container.ContainedEntities)
                    {
                        if (!entities.Contains(ent))
                            entities.Add(ent);
                    }
                }
            }

            if ((visibility & MenuVisibility.InContainer) != 0)
            {
                // This is inefficient, but I'm lazy and CBF implementing my own recursive container method. Note that
                // this might actually fail to add the contained children of some entities in the menu. E.g., an entity
                // with a large sprite aabb, but small broadphase might appear in the menu, but have its children added
                // by this.
                var flags = LookupFlags.All & ~LookupFlags.Sensors;
                foreach (var e in _lookup.GetEntitiesInRange(targetPos, _lookupSize, flags: flags))
                {
                    if (!entities.Contains(e))
                        entities.Add(e);
                }
            }

            // Do we have to do FoV checks?
            if ((visibility & MenuVisibility.NoFov) == 0)
            {
                TryComp(player, out ExaminerComponent? examiner);
                for (var i = entities.Count - 1; i >= 0; i--)
                {
                    if (!_examine.CanExamine(player, targetPos, e => e == player, entities[i], examiner))
                        entities.RemoveSwap(i);
                }
            }

            if ((visibility & MenuVisibility.Invisible) != 0)
                return entities.Count != 0;

            for (var i = entities.Count - 1; i >= 0; i--)
            {
                if (_tagSystem.HasTag(entities[i], HideContextMenuTag))
                    entities.RemoveSwap(i);
            }

            // Unless we added entities in containers, every entity should already have a visible sprite due to
            // the fact that we used the sprite tree query.
            if (container == null && (visibility & MenuVisibility.InContainer) == 0)
                return entities.Count != 0;

            var spriteQuery = GetEntityQuery<SpriteComponent>();
            for (var i = entities.Count - 1; i >= 0; i--)
            {
                if (!spriteQuery.TryGetComponent(entities[i], out var spriteComponent) || !spriteComponent.Visible)
                    entities.RemoveSwap(i);
            }

            return entities.Count != 0;
        }

        /// <summary>
        ///     Ask the server to send back a list of server-side verbs, and for now return an incomplete list of verbs
        ///     (only those defined locally).
        /// </summary>
        public SortedSet<Verb> GetVerbs(NetEntity target, EntityUid user, List<Type> verbTypes, out List<VerbCategory> extraCategories, bool force = false)
        {
            if (!target.IsClientSide())
                RaiseNetworkEvent(new RequestServerVerbsEvent(target, verbTypes, adminRequest: force));

            // Some admin menu interactions will try get verbs for entities that have not yet been sent to the player.
            if (!TryGetEntity(target, out var local))
            {
                extraCategories = new();
                return new();
            }

            return GetLocalVerbs(local.Value, user, verbTypes, out extraCategories, force);
        }


        /// <summary>
        ///     Execute actions associated with the given verb.
        /// </summary>
        /// <remarks>
        ///     Unless this is a client-exclusive verb, this will also tell the server to run the same verb.
        /// </remarks>
        public void ExecuteVerb(EntityUid target, Verb verb)
        {
            ExecuteVerb(GetNetEntity(target), verb);
        }

        /// <summary>
        ///     Execute actions associated with the given verb.
        /// </summary>
        /// <remarks>
        ///     Unless this is a client-exclusive verb, this will also tell the server to run the same verb.
        /// </remarks>
        public void ExecuteVerb(NetEntity target, Verb verb)
        {
            if ( _playerManager.LocalEntity is not {} user)
                return;

            // is this verb actually valid?
            if (verb.Disabled)
            {
                // maybe send an informative pop-up message.
                if (!string.IsNullOrWhiteSpace(verb.Message))
                    _popupSystem.PopupEntity(FormattedMessage.RemoveMarkupOrThrow(verb.Message), user);

                return;
            }

            if (verb.ClientExclusive || target.IsClientSide())
                // is this a client exclusive (gui) verb?
                ExecuteVerb(verb, user, GetEntity(target));
            else
                EntityManager.RaisePredictiveEvent(new ExecuteVerbEvent(target, verb));
        }

        private void HandleVerbResponse(VerbsResponseEvent msg)
        {
            OnVerbsResponse?.Invoke(msg);
        }
    }
}
