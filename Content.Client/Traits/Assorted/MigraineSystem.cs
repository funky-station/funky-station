// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;
using Content.Shared.Traits.Assorted;

namespace Content.Client.Traits.Assorted
{
    /// <summary>
    /// Client system for migraine visual overlay and audio effects.
    /// </summary>
    public sealed class MigraineSystem : EntitySystem
    {
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private MigraineOverlay _overlay = default!;
        private bool _playedStartSound = false;

        public override void Initialize()
        {
            base.Initialize();

            _overlay = new MigraineOverlay();

            SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);

            // Add overlay if player is already attached
            if (_playerManager.LocalSession?.AttachedEntity != null)
            {
                _overlayMan.AddOverlay(_overlay);
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_timing.IsFirstTimePredicted)
                return;

            var localEntity = _playerManager.LocalSession?.AttachedEntity;
            if (localEntity == null)
            {
                _playedStartSound = false;
                return;
            }

            // Play migraine start sound for the player
            if (_entityManager.TryGetComponent<MigraineComponent>(localEntity, out var _))
            {
                if (!_playedStartSound)
                {
                    _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_Funkystation/Effects/Migraine/migraine.ogg"),
                        localEntity.Value, localEntity.Value);
                    _playedStartSound = true;
                }
            }
            else
            {
                _playedStartSound = false;
            }
        }

        private void OnPlayerAttached(LocalPlayerAttachedEvent args)
        {
            _overlayMan.AddOverlay(_overlay);
        }

        private void OnPlayerDetached(LocalPlayerDetachedEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
