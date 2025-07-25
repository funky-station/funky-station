// SPDX-FileCopyrightText: 2021 Charlese2 <knightside@hotmail.com>
// SPDX-FileCopyrightText: 2021 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 CommieFlowers <rasmus.cedergren@hotmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jessica M <jessica@jessicamaybe.com>
// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Tomás Alves <tomasalves35@gmail.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Naive817 <31364560+Naive817@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Flash.Components;
using Content.Shared.Flash.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Flashbang;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Flash;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared.Traits.Assorted;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Examine;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using InventoryComponent = Content.Shared.Inventory.InventoryComponent;
using Robust.Shared.Prototypes;
using Content.Shared._EE.Overlays.Switchable; // Funky - Thermal/nightvision flash vulnerability

namespace Content.Server.Flash
{
    internal sealed class FlashSystem : SharedFlashSystem
    {
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly SharedChargesSystem _charges = default!;
        [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly ExamineSystemShared _examine = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly StunSystem _stun = default!;
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

        private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FlashComponent, MeleeHitEvent>(OnFlashMeleeHit);
            // ran before toggling light for extra-bright lantern
            SubscribeLocalEvent<FlashComponent, UseInHandEvent>(OnFlashUseInHand, before: new[] { typeof(HandheldLightSystem) });
            SubscribeLocalEvent<InventoryComponent, FlashAttemptEvent>(OnInventoryFlashAttempt);
            SubscribeLocalEvent<FlashImmunityComponent, FlashAttemptEvent>(OnFlashImmunityFlashAttempt);
            SubscribeLocalEvent<PermanentBlindnessComponent, FlashAttemptEvent>(OnPermanentBlindnessFlashAttempt);
            SubscribeLocalEvent<TemporaryBlindnessComponent, FlashAttemptEvent>(OnTemporaryBlindnessFlashAttempt);
        }

        private void OnFlashMeleeHit(EntityUid uid, FlashComponent comp, MeleeHitEvent args)
        {
            if (!args.IsHit ||
                !args.HitEntities.Any() ||
                !UseFlash(uid, comp, args.User))
            {
                return;
            }

            args.Handled = true;
            foreach (var e in args.HitEntities)
            {
                Flash(e, args.User, uid, comp.FlashDuration, comp.SlowTo, melee: true, stunDuration: comp.MeleeStunDuration);
            }
        }

        private void OnFlashUseInHand(EntityUid uid, FlashComponent comp, UseInHandEvent args)
        {
            if (args.Handled || !UseFlash(uid, comp, args.User))
                return;

            args.Handled = true;
            FlashArea(uid, args.User, comp.Range, comp.AoeFlashDuration, comp.SlowTo, true, comp.Probability);
        }

        private bool UseFlash(EntityUid uid, FlashComponent comp, EntityUid user)
        {
            if (comp.Flashing)
                return false;

            TryComp<LimitedChargesComponent>(uid, out var charges);
            if (_charges.IsEmpty(uid, charges))
                return false;

            _charges.UseCharge(uid, charges);
            _audio.PlayPvs(comp.Sound, uid);
            comp.Flashing = true;
            _appearance.SetData(uid, FlashVisuals.Flashing, true);

            if (_charges.IsEmpty(uid, charges))
            {
                _appearance.SetData(uid, FlashVisuals.Burnt, true);
                _tag.AddTag(uid, TrashTag);
                _popup.PopupEntity(Loc.GetString("flash-component-becomes-empty"), user);
            }

            uid.SpawnTimer(400, () =>
            {
                _appearance.SetData(uid, FlashVisuals.Flashing, false);
                comp.Flashing = false;
            });

            return true;
        }

        public void Flash(EntityUid target,
            EntityUid? user,
            EntityUid? used,
            float flashDuration,
            float slowTo,
            bool displayPopup = true,
            bool melee = false,
            TimeSpan? stunDuration = null,
            bool revFlash = false)
        {
            var attempt = new FlashAttemptEvent(target, user, used);
            RaiseLocalEvent(target, attempt, true);

            if (attempt.Cancelled)
                return;

            // Goobstation start
            var multiplierEv = new FlashDurationMultiplierEvent();
            RaiseLocalEvent(target, multiplierEv);
            var multiplier = multiplierEv.Multiplier;
            // Goobstation end

            // don't paralyze, slowdown or convert to rev if the target is immune to flashes
            if (!_statusEffectsSystem.TryAddStatusEffect<FlashedComponent>(target, FlashedKey, TimeSpan.FromSeconds(flashDuration * multiplier / 1000f), true)) // Goob edit
                return;

            if (stunDuration != null)
            {
                _stun.TryKnockdown(target, stunDuration.Value * multiplier, true); // Goob edit
            }
            else
            {
                _stun.TrySlowdown(target, TimeSpan.FromSeconds(flashDuration * multiplier / 1000f), true,
                slowTo, slowTo); // Goob edit
            }

            if (displayPopup && user != null && target != user && Exists(user.Value))
            {
                _popup.PopupEntity(Loc.GetString("flash-component-user-blinds-you",
                    ("user", Identity.Entity(user.Value, EntityManager))), target, target);
            }

            if (melee)
            {
                if (user == null)
                    return;

                if (used == null)
                    return;

                var ev = new AfterFlashedEvent(target, user, used);
                RaiseLocalEvent(user.Value, ref ev);
                RaiseLocalEvent(used.Value, ref ev);
            }
        }

        public override void FlashArea(Entity<FlashComponent?> source, EntityUid? user, float range, float duration, float slowTo = 0.8f, bool displayPopup = false, float probability = 1f, SoundSpecifier? sound = null)
        {
            var transform = Transform(source);
            var mapPosition = _transform.GetMapCoordinates(transform);
            var statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();
            var damagedByFlashingQuery = GetEntityQuery<DamagedByFlashingComponent>();

            foreach (var entity in _entityLookup.GetEntitiesInRange(transform.Coordinates, range))
            {
                if (!_random.Prob(probability))
                    continue;

                // Is the entity affected by the flash either through status effects or by taking damage?
                if (!statusEffectsQuery.HasComponent(entity) && !damagedByFlashingQuery.HasComponent(entity))
                    continue;

                // Check for entites in view
                // put damagedByFlashingComponent in the predicate because shadow anomalies block vision.
                if (!_examine.InRangeUnOccluded(entity, mapPosition, range, predicate: (e) => damagedByFlashingQuery.HasComponent(e)))
                    continue;

                // They shouldn't have flash removed in between right?
                Flash(entity, user, source, duration, slowTo, displayPopup);

                var distance = (mapPosition.Position - _transform.GetMapCoordinates(entity).Position).Length(); // Goobstation
                RaiseLocalEvent(source, new AreaFlashEvent(range, distance, entity)); // Goobstation
            }

            _audio.PlayPvs(sound, source, AudioParams.Default.WithVolume(1f).WithMaxDistance(3f));
        }

        // funkystation: copy and paste of above, cry about it
        public void RevolutionaryFlashArea(Entity<FlashComponent?> source, EntityUid? user, float range, float duration, float slowTo = 0.8f, bool displayPopup = false, float probability = 1f, SoundSpecifier? sound = null)
        {
            var transform = Transform(source);
            var mapPosition = _transform.GetMapCoordinates(transform);
            var statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();
            var damagedByFlashingQuery = GetEntityQuery<DamagedByFlashingComponent>();

            foreach (var entity in _entityLookup.GetEntitiesInRange(transform.Coordinates, range))
            {
                if (!_random.Prob(probability))
                    continue;

                // Is the entity affected by the flash either through status effects or by taking damage?
                if (!statusEffectsQuery.HasComponent(entity) && !damagedByFlashingQuery.HasComponent(entity))
                    continue;

                // Check for entites in view
                // put damagedByFlashingComponent in the predicate because shadow anomalies block vision.
                if (!_examine.InRangeUnOccluded(entity, mapPosition, range, predicate: (e) => damagedByFlashingQuery.HasComponent(e)))
                    continue;

                // They shouldn't have flash removed in between right?
                Flash(entity, user, source, duration, slowTo, displayPopup, true, null, true);
            }

            _audio.PlayPvs(sound, source, AudioParams.Default.WithVolume(1f).WithMaxDistance(3f));
        }

        private void OnInventoryFlashAttempt(EntityUid uid, InventoryComponent component, FlashAttemptEvent args)
        {
            foreach (var slot in new[] { "head", "eyes", "mask" })
            {
                if (args.Cancelled)
                    break;
                if (_inventory.TryGetSlotEntity(uid, slot, out var item, component))
                    RaiseLocalEvent(item.Value, args, true);
            }
        }

        private void OnFlashImmunityFlashAttempt(EntityUid uid, FlashImmunityComponent component, FlashAttemptEvent args)
        {
            if (!component.Enabled)
                return;
            if (CheckFlashVulnerability(args.Target)) // Funky edit - Check if flash target has vulnerability from thermal/nightvision.
                return; // If flash vulnerability returns true, flash immunity will have no effect.
            args.Cancel();
        }

        // Funky edit starts
        private bool CheckFlashVulnerability(EntityUid target)
        {
            foreach (var slot in new[] { "head", "eyes", "mask" })
            {
                if (_inventory.TryGetSlotEntity(target, slot, out var item))
                {
                    if (TryComp<ThermalVisionComponent>(item, out var thermalvision) &&
                        thermalvision.IsActive == true &&
                        thermalvision.FlashDurationMultiplier > 1)  // If the thermal/nightvision comp has any flash multiplier, ignore
                                                                    // flash immunity. If a flash multiplier is not set on the comp (or is
                        return true;                                // set to 1 or lower), flash immunity will work as normal.

                    if (TryComp<NightVisionComponent>(item, out var nightvision) &&
                        nightvision.IsActive == true &&
                        nightvision.FlashDurationMultiplier > 1)

                        return true;
                }
            }
            if (TryComp<ThermalVisionComponent>(target, out var selfthermalvision) &&   // Copypasted but checks the flash target instead of their
                        selfthermalvision.IsActive == true &&                           // equipped items, in case they're a ling with enhanced
                        selfthermalvision.FlashDurationMultiplier > 1)                  // eyesight. There's probably a cleaner way to do this.

                return true;

            if (TryComp<NightVisionComponent>(target, out var selfnightvision) &&
                selfnightvision.IsActive == true &&
                selfnightvision.FlashDurationMultiplier > 1)

                return true;

            return false;
        }
        // Funky edit ends

        private void OnPermanentBlindnessFlashAttempt(EntityUid uid, PermanentBlindnessComponent component, FlashAttemptEvent args)
        {
            // check for total blindness
            if (component.Blindness == 0)
                args.Cancel();
        }

        private void OnTemporaryBlindnessFlashAttempt(EntityUid uid, TemporaryBlindnessComponent component, FlashAttemptEvent args)
        {
            args.Cancel();
        }
    }

    /// <summary>
    ///     Called before a flash is used to check if the attempt is cancelled by blindness, items or FlashImmunityComponent.
    ///     Raised on the target hit by the flash, the user of the flash and the flash used.
    /// </summary>
    public sealed class FlashAttemptEvent : CancellableEntityEventArgs
    {
        public readonly EntityUid Target;
        public readonly EntityUid? User;
        public readonly EntityUid? Used;

        public FlashAttemptEvent(EntityUid target, EntityUid? user, EntityUid? used)
        {
            Target = target;
            User = user;
            Used = used;
        }
    }
    /// <summary>
    ///     Called after a flash is used via melee on another person to check for rev conversion.
    ///     Raised on the target hit by the flash, the user of the flash and the flash used.
    /// </summary>
    [ByRefEvent]
    public readonly struct AfterFlashedEvent
    {
        public readonly EntityUid Target;
        public readonly EntityUid? User;
        public readonly EntityUid? Used;

        public AfterFlashedEvent(EntityUid target, EntityUid? user, EntityUid? used)
        {
            Target = target;
            User = user;
            Used = used;
        }
    }
}
