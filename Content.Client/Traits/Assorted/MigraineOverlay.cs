// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Timing;
using Robust.Shared.Enums;

namespace Content.Client.Traits.Assorted
{
    public sealed class MigraineOverlay : Overlay
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        public override bool RequestScreenTexture => true;
        public override OverlaySpace Space => OverlaySpace.WorldSpace;

        private readonly ShaderInstance _cataractsShader;

        private bool _isFading = false;
        private float _localCurrentBlur = 0f;
        private float _localPulseAccumulator = 0f;
        private float _localPulseFrequency = 0.4f;
        private float _localPulseAmplitude = 0.8f;
        private float _localRampDownSpeed = 2f;
        private bool _localUseSoftShader = false; // i dont remember if this actually does anything
        private float _localSoftness = 0.45f;

        public MigraineOverlay()
        {
            IoCManager.InjectDependencies(this);
            _cataractsShader = _prototypeManager.Index<ShaderPrototype>("Cataracts").InstanceUnique();
        }

        protected override bool BeforeDraw(in OverlayDrawArgs args)
        {
            if (!_entityManager.TryGetComponent(_playerManager.LocalSession?.AttachedEntity, out EyeComponent? eyeComp))
                return false;

            if (args.Viewport.Eye != eyeComp.Eye)
                return false;

            var playerEntity = _playerManager.LocalSession?.AttachedEntity;

            if (playerEntity == null)
                return false;

            // If the MigraineComponent exists, use it and reset fading state.
            if (_entityManager.TryGetComponent<MigraineComponent>(playerEntity, out var migraine))
            {
                _isFading = false;
                _localCurrentBlur = migraine.CurrentBlur;
                _localPulseAccumulator = migraine.PulseAccumulator;
                _localPulseFrequency = migraine.PulseFrequency;
                _localPulseAmplitude = migraine.PulseAmplitude;
                _localRampDownSpeed = migraine.RampDownSpeed > 0f ? migraine.RampDownSpeed : migraine.RampUpSpeed;
                _localUseSoftShader = migraine.UseSoftShader;
                _localSoftness = migraine.Softness;

                var pulseOffset = MathF.Sin(_localPulseAccumulator * _localPulseFrequency * MathF.PI * 2f) * _localPulseAmplitude;
                var strength = _localCurrentBlur + pulseOffset;
                if (_localUseSoftShader)
                    strength *= _localSoftness;

                if (strength <= 0f)
                    return false;

                return true;
            }

            if (_localCurrentBlur > 0.001f)
            {
                _isFading = true;
            }

            return false;
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture == null)
                return;

            var playerEntity = _playerManager.LocalSession?.AttachedEntity;
            if (playerEntity == null)
                return;

            var viewport = args.WorldBounds;

            var zoom = 1.0f;
            if (_entityManager.TryGetComponent<EyeComponent>(playerEntity, out var eyeComponent))
            {
                zoom = eyeComponent.Zoom.X;
            }

            var frameTime = (float) _gameTiming.FrameTime.TotalSeconds;

            if (_entityManager.TryGetComponent<MigraineComponent>(playerEntity, out var migraine2))
            {
                _localPulseAccumulator = migraine2.PulseAccumulator;
                _localCurrentBlur = migraine2.CurrentBlur;
                _localPulseFrequency = migraine2.PulseFrequency;
                _localPulseAmplitude = migraine2.PulseAmplitude;
                _localUseSoftShader = migraine2.UseSoftShader;
                _localSoftness = migraine2.Softness;
                _localRampDownSpeed = migraine2.RampDownSpeed > 0f ? migraine2.RampDownSpeed : migraine2.RampUpSpeed;
            }
            else
            {
                _isFading = true;
                _localPulseAccumulator += frameTime;
                _localCurrentBlur = MathHelper.Lerp(_localCurrentBlur, 0f, frameTime * _localRampDownSpeed);
                _localPulseAmplitude = MathHelper.Lerp(_localPulseAmplitude, 0f, frameTime * _localRampDownSpeed);
            }

            var pulseOffset = MathF.Sin(_localPulseAccumulator * _localPulseFrequency * MathF.PI * 2f) * _localPulseAmplitude;
            var strength = _localCurrentBlur + pulseOffset;
            if (_localUseSoftShader)
                strength *= _localSoftness;

            strength = Math.Clamp(strength, 0f, BlurryVisionComponent.MaxMagnitude);

            var normalized = (float) Math.Pow(Math.Min(strength / BlurryVisionComponent.MaxMagnitude, 1.0f), BlurryVisionComponent.DefaultCorrectionPower);

            _cataractsShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            _cataractsShader.SetParameter("LIGHT_TEXTURE", args.Viewport.LightRenderTarget.Texture);
            _cataractsShader.SetParameter("Zoom", zoom);

            _cataractsShader.SetParameter("DistortionScalar", (float) Math.Pow(normalized, 2.0f));
            _cataractsShader.SetParameter("CloudinessScalar", (float) Math.Pow(normalized, 1.0f));

            var worldHandle = args.WorldHandle;
            worldHandle.UseShader(_cataractsShader);
            worldHandle.DrawRect(viewport, Color.White);
            worldHandle.UseShader(null);

            if (_isFading && _localCurrentBlur <= 0.001f && _localPulseAmplitude <= 0.001f)
            {
                _isFading = false;
                _localPulseAccumulator = 0f;
            }
        }
    }
}
