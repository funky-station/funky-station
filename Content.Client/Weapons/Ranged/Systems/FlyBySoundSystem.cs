// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed class FlyBySoundSystem : SharedFlyBySoundSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FlyBySoundComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, FlyBySoundComponent component, ref StartCollideEvent args)
    {
        var attachedEnt = _player.LocalEntity;

        // If it's not our ent or we shot it.
        if (attachedEnt == null ||
            args.OtherEntity != attachedEnt ||
            TryComp<ProjectileComponent>(uid, out var projectile) &&
            projectile.Shooter == attachedEnt)
        {
            return;
        }

        if (args.OurFixtureId != FlyByFixture ||
            !_random.Prob(component.Prob))
        {
            return;
        }

        // Play attached to our entity because the projectile may immediately delete or the likes.
        _audio.PlayPredicted(component.Sound, attachedEnt.Value, attachedEnt.Value);
    }
}
