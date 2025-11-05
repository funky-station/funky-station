// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 forkeyboards <91704530+forkeyboards@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 vectorassembly <vectorassembly@icloud.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.Humanoid.Events;
using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Content.Shared.Whitelist;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<ProfileWithEntityLoadFinishedEvent>(OnProfileLoadFinished);
    }

    private void OnProfileLoadFinished(ProfileWithEntityLoadFinishedEvent ev)
    {
        ApplyTraitsToEntity(ev.Uid, ev.Profile.TraitPreferences);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.JobId == null ||
            !_prototypeManager.TryIndex<JobPrototype>(args.JobId, out var protoJob) ||
            !protoJob.ApplyTraits)
        {
            return;
        }

        ApplyTraitsToEntity(args.Mob, args.Profile.TraitPreferences);
    }

    /// <summary>
    /// Applies all valid traits from the given list to the target entity.
    /// </summary>
    public void ApplyTraitsToEntity(EntityUid target, IReadOnlySet<ProtoId<TraitPrototype>> traitIds)
    {
        foreach (var traitId in traitIds)
        {
            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Warning($"No trait found with ID {traitId}!");
                continue;
            }

            if (_whitelistSystem.IsWhitelistFail(traitPrototype.Whitelist, target) ||
                _whitelistSystem.IsBlacklistPass(traitPrototype.Blacklist, target))
                continue;

            if (TryComp<HumanoidAppearanceComponent>(target, out var appearance) &&
                traitPrototype.SpeciesRestrictions != null &&
                traitPrototype.SpeciesRestrictions.Contains(appearance.Species))
                continue;

            EntityManager.AddComponents(target, traitPrototype.Components, false);

            if (traitPrototype.TraitGear == null)
                continue;

            if (!TryComp(target, out HandsComponent? handsComponent))
                continue;

            var coords = Transform(target).Coordinates;
            var inhandEntity = EntityManager.SpawnEntity(traitPrototype.TraitGear, coords);
            _sharedHandsSystem.TryPickup(target, inhandEntity, checkActionBlocker: false, handsComp: handsComponent);
        }
    }
}
