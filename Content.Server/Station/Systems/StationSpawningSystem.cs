// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PoTeletubby <151896601+PoTeletubby@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GreyMario <mariomister541@gmail.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 ferynn <117872973+ferynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Access.Systems;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Server.Mind.Commands;
using Content.Server.PDA;
using Content.Server.Station.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Clothing;
using Content.Shared.DetailExaminable;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.NameIdentifier;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Station;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server._EinsteinEngines.Silicon.IPC; // Goobstation
using Content.Server.Access.Components; // Funkystation

namespace Content.Server.Station.Systems;

/// <summary>
/// Manages spawning into the game, tracking available spawn points.
/// Also provides helpers for spawning in the player's mob.
/// </summary>
[PublicAPI]
public sealed class StationSpawningSystem : SharedStationSpawningSystem
{
    [Dependency] private readonly SharedAccessSystem _accessSystem = default!;
    [Dependency] private readonly ActorSystem _actors = default!;
    [Dependency] private readonly IdCardSystem _cardSystem = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly PdaSystem _pdaSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InternalEncryptionKeySpawner _internalEncryption = default!; // Goobstation
    private bool _randomizeCharacters;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configurationManager, CCVars.ICRandomCharacters, e => _randomizeCharacters = e, true);
    }

    /// <summary>
    /// Attempts to spawn a player character onto the given station.
    /// </summary>
    /// <param name="station">Station to spawn onto.</param>
    /// <param name="job">The job to assign, if any.</param>
    /// <param name="profile">The character profile to use, if any.</param>
    /// <param name="stationSpawning">Resolve pattern, the station spawning component for the station.</param>
    /// <returns>The resulting player character, if any.</returns>
    /// <exception cref="ArgumentException">Thrown when the given station is not a station.</exception>
    /// <remarks>
    /// This only spawns the character, and does none of the mind-related setup you'd need for it to be playable.
    /// </remarks>
    public EntityUid? SpawnPlayerCharacterOnStation(EntityUid? station, ProtoId<JobPrototype>? job, HumanoidCharacterProfile? profile, StationSpawningComponent? stationSpawning = null)
    {
        if (station != null && !Resolve(station.Value, ref stationSpawning))
            throw new ArgumentException("Tried to use a non-station entity as a station!", nameof(station));

        var ev = new PlayerSpawningEvent(job, profile, station);

        RaiseLocalEvent(ev);
        DebugTools.Assert(ev.SpawnResult is { Valid: true } or null);

        return ev.SpawnResult;
    }

    //TODO: Figure out if everything in the player spawning region belongs somewhere else.
    #region Player spawning helpers

    /// <summary>
    /// Spawns in a player's mob according to their job and character information at the given coordinates.
    /// Used by systems that need to handle spawning players.
    /// </summary>
    /// <param name="coordinates">Coordinates to spawn the character at.</param>
    /// <param name="job">Job to assign to the character, if any.</param>
    /// <param name="profile">Appearance profile to use for the character.</param>
    /// <param name="station">The station this player is being spawned on.</param>
    /// <param name="entity">The entity to use, if one already exists.</param>
    /// <returns>The spawned entity</returns>
    public EntityUid SpawnPlayerMob(
        EntityCoordinates coordinates,
        ProtoId<JobPrototype>? job,
        HumanoidCharacterProfile? profile,
        EntityUid? station,
        EntityUid? entity = null)
    {
        _prototypeManager.TryIndex(job ?? string.Empty, out var prototype);
        RoleLoadout? loadout = null;
        string? nameOverride = null;

        // Need to get the loadout up-front to handle names if we use an entity spawn override.
        var jobLoadout = LoadoutSystem.GetJobPrototype(prototype?.ID);

        if (_prototypeManager.TryIndex(jobLoadout, out RoleLoadoutPrototype? roleProto))
        {
            profile?.Loadouts.TryGetValue(jobLoadout, out loadout);
            if (loadout != null)
            {
                if (roleProto.CanCustomizeName)
                {
                    nameOverride = loadout.EntityName;
                }
            }
            // Set to default if not present
            if (loadout == null)
            {
                loadout = new RoleLoadout(jobLoadout);
                loadout.SetDefault(profile, _actors.GetSession(entity), _prototypeManager);
            }
        }

        // If we're not spawning a humanoid, we're gonna exit early without doing all the humanoid stuff.
        if (prototype?.JobEntity != null)
        {
            DebugTools.Assert(entity is null);
            var jobEntity = EntityManager.SpawnEntity(prototype.JobEntity, coordinates);
            MakeSentientCommand.MakeSentient(jobEntity, EntityManager);

            // Make sure custom names get handled, what is gameticker control flow whoopy.
            if (loadout != null)
            {
                EquipRoleName(jobEntity, loadout, roleProto!);
            }

            DoJobSpecials(job, jobEntity);
            _identity.QueueIdentityUpdate(jobEntity);
            // #Goobstation - Borg Preferred Name
            if (profile != null && (prototype.ID == "Borg" || prototype.ID == "StationAi"))
            {
                var name = profile.BorgName;
                if (TryComp<NameIdentifierComponent>(jobEntity, out var nameIdentifier) && (prototype.ID !="StationAi"))
                    name = $"{name} {nameIdentifier.FullIdentifier}";

                _metaSystem.SetEntityName(jobEntity, name);
            }
            return jobEntity;
        }

        string speciesId;
        if (_randomizeCharacters)
        {
            var weightId = _configurationManager.GetCVar(CCVars.ICRandomSpeciesWeights);
            var weights = _prototypeManager.Index<WeightedRandomSpeciesPrototype>(weightId);
            speciesId = weights.Pick(_random);
        }
        else if (profile != null)
        {
            speciesId = profile.Species;
        }
        else
        {
            speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
        }

        if (!_prototypeManager.TryIndex<SpeciesPrototype>(speciesId, out var species))
            throw new ArgumentException($"Invalid species prototype was used: {speciesId}");

        entity ??= Spawn(species.Prototype, coordinates);

        if (_randomizeCharacters)
        {
            profile = HumanoidCharacterProfile.RandomWithSpecies(speciesId);
        }

        if (loadout != null)
        {
            EquipRoleLoadout(entity.Value, loadout, roleProto!);
        }

        if (prototype?.StartingGear != null)
        {
            var startingGear = _prototypeManager.Index<StartingGearPrototype>(prototype.StartingGear);
            EquipStartingGear(entity.Value, startingGear, raiseEvent: false);
            _internalEncryption.TryInsertEncryptionKey(entity.Value, startingGear, EntityManager); // Goobstation
        }

        var gearEquippedEv = new StartingGearEquippedEvent(entity.Value);
        RaiseLocalEvent(entity.Value, ref gearEquippedEv);

        if (profile != null)
        {
            if (prototype != null)
            {
                if (nameOverride != null)
                {
                    SetPdaAndIdCardData(entity.Value, nameOverride, prototype, station);
                    _metaSystem.SetEntityName(entity.Value, nameOverride);
                }
                else
                {
                    SetPdaAndIdCardData(entity.Value, profile.Name, prototype, station);
                    _metaSystem.SetEntityName(entity.Value, profile.Name);
                }
            }


            _humanoidSystem.LoadProfile(entity.Value, profile);

            if (profile.FlavorText != "" && _configurationManager.GetCVar(CCVars.FlavorText))
            {
                AddComp<DetailExaminableComponent>(entity.Value).Content = profile.FlavorText;
            }
        }

        DoJobSpecials(job, entity.Value);
        _identity.QueueIdentityUpdate(entity.Value);
        return entity.Value;
    }

    private void DoJobSpecials(ProtoId<JobPrototype>? job, EntityUid entity)
    {
        if (!_prototypeManager.TryIndex(job ?? string.Empty, out JobPrototype? prototype))
            return;

        foreach (var jobSpecial in prototype.Special)
        {
            jobSpecial.AfterEquip(entity);
        }
    }

    /// <summary>
    /// Sets the ID card and PDA name, job, and access data.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="characterName">Character name to use for the ID.</param>
    /// <param name="jobPrototype">Job prototype to use for the PDA and ID.</param>
    /// <param name="station">The station this player is being spawned on.</param>
    public void SetPdaAndIdCardData(EntityUid entity, string characterName, JobPrototype jobPrototype, EntityUid? station)
    {
        if (!InventorySystem.TryGetSlotEntity(entity, "id", out var idUid))
            return;

        var cardId = idUid.Value;
        if (TryComp<PdaComponent>(idUid, out var pdaComponent) && pdaComponent.ContainedId != null)
            cardId = pdaComponent.ContainedId.Value;

        if (!TryComp<IdCardComponent>(cardId, out var card))
            return;

        _cardSystem.TryChangeFullName(cardId, characterName, card);
        _cardSystem.TryChangeJobTitle(cardId, jobPrototype.LocalizedName, card);

        if (_prototypeManager.TryIndex(jobPrototype.Icon, out var jobIcon))
            _cardSystem.TryChangeJobIcon(cardId, jobIcon, card);

        var extendedAccess = false;
        if (station != null)
        {
            var data = Comp<StationJobsComponent>(station.Value);
            extendedAccess = data.ExtendedAccess;
        }

        _accessSystem.SetAccessToJob(cardId, jobPrototype, extendedAccess);

        // funkystation: prevent setting PresetIdCard accesses; we just GOT our accesses, remember?
        if (TryComp<PresetIdCardComponent>(cardId, out _))
            RemComp<PresetIdCardComponent>(cardId);

        if (pdaComponent != null)
            _pdaSystem.SetOwner(idUid.Value, pdaComponent, entity, characterName);
    }


    #endregion Player spawning helpers
}

/// <summary>
/// Ordered broadcast event fired on any spawner eligible to attempt to spawn a player.
/// This event's success is measured by if SpawnResult is not null.
/// You should not make this event's success rely on random chance.
/// This event is designed to use ordered handling. You probably want SpawnPointSystem to be the last handler.
/// </summary>
[PublicAPI]
public sealed class PlayerSpawningEvent : EntityEventArgs
{
    /// <summary>
    /// The entity spawned, if any. You should set this if you succeed at spawning the character, and leave it alone if it's not null.
    /// </summary>
    public EntityUid? SpawnResult;
    /// <summary>
    /// The job to use, if any.
    /// </summary>
    public readonly ProtoId<JobPrototype>? Job;
    /// <summary>
    /// The profile to use, if any.
    /// </summary>
    public readonly HumanoidCharacterProfile? HumanoidCharacterProfile;
    /// <summary>
    /// The target station, if any.
    /// </summary>
    public readonly EntityUid? Station;

    public PlayerSpawningEvent(ProtoId<JobPrototype>? job, HumanoidCharacterProfile? humanoidCharacterProfile, EntityUid? station)
    {
        Job = job;
        HumanoidCharacterProfile = humanoidCharacterProfile;
        Station = station;
    }
}
