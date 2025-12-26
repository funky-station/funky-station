// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedMansusGraspSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;

    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public bool TryApplyGraspEffectAndMark(EntityUid user,
        HereticComponent hereticComp,
        EntityUid target,
        EntityUid? grasp,
        out bool triggerGrasp)
    {
        triggerGrasp = true;

        if (hereticComp.CurrentPath == null)
            return true;

        if (hereticComp.PathStage >= 2)
        {
            if (!ApplyGraspEffect((user, hereticComp), target, grasp, out var applyMark, out triggerGrasp))
                return false;

            if (!applyMark)
                return true;
        }

        if (hereticComp.PathStage >= 4 && HasComp<StatusEffectsComponent>(target))
        {
            var markComp = EnsureComp<HereticCombatMarkComponent>(target);
            markComp.DisappearTime = markComp.MaxDisappearTime;
            markComp.Path = hereticComp.CurrentPath;
            markComp.Repetitions = hereticComp.CurrentPath == "Ash" ? 5 : 1;
            Dirty(target, markComp);

        }

        return true;
    }

    public bool ApplyGraspEffect(Entity<HereticComponent> user,
        EntityUid target,
        EntityUid? grasp,
        out bool applyMark,
        out bool triggerGrasp)
    {
        applyMark = true;
        triggerGrasp = true;
        var (performer, heretic) = user;

        switch (heretic.CurrentPath)
        {
            case "Ash":
            {
                var timeSpan = TimeSpan.FromSeconds(5f);
                _statusEffect.TryAddStatusEffect(target,
                    TemporaryBlindnessSystem.BlindingStatusEffect,
                    timeSpan,
                    false,
                    TemporaryBlindnessSystem.BlindingStatusEffect);
                break;
            }

            case "Lock":
            {
                if (!TryComp<DoorComponent>(target, out var door))
                    break;

                if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                    _door.SetBoltsDown((target, doorBolt), false);

                _door.StartOpening(target, door);
                _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hereticknock.ogg"),
                    target,
                    user);
                break;
            }

            case "Flesh":
            {
                if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState != MobState.Alive &&
                    !HasComp<BorgChassisComponent>(target))
                {
                    if (HasComp<GhoulComponent>(target))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-ghoul"), user, user);
                        break;
                    }

                    if (!_mind.TryGetMind(target, out _, out _))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-no-mind"), user, user);
                        break;
                    }

                    var ghoul = _compFactory.GetComponent<GhoulComponent>();
                    ghoul.BoundHeretic = performer;
                    ghoul.GiveBlade = true;

                    AddComp(target, ghoul);
                    applyMark = false;
                    triggerGrasp = false;
                }

                break;
            }

            default:

                return true;
        }
        return true;
    }

}

