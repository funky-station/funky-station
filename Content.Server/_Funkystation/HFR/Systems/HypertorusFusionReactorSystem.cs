// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Funkystation.Atmos.HFR;
using Content.Server.Power.EntitySystems;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Administration.Logs;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Funkystation.Atmos.Prototypes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Speech;
using Content.Shared.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Server.Audio;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Shared.Construction.Components;
using Content.Server._Funkystation.Atmos.Systems;
using Content.Server.Lightning;
using Content.Server.Singularity.EntitySystems;
using Content.Shared.Radiation.Components;
using Robust.Shared.Spawners;
using System.Text;
using Robust.Shared.Map;
using Content.Server.Atmos.Components;
using Content.Shared.Atmos.Components;
using Content.Server.Power.Components;

namespace Content.Server._Funkystation.Atmos.HFR.Systems
{
    public sealed partial class HypertorusFusionReactorSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly RadioSystem _radioSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IAdminLogManager _logSystem = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
        [Dependency] private readonly EmpSystem _empSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly AtmosphereSystem _atmosSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly HFRConsoleSystem _hfrConsoleSystem = default!;
        [Dependency] private readonly LightningSystem _lightning = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiver = default!;
        [Dependency] private readonly GravityWellSystem _gravityWell = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
        [Dependency] private readonly SharedMapSystem _mapSystem = default!;
        [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public static readonly Vector2i[] DiagonalOffsets = [new(1, 1), new(-1, 1), new(-1, -1), new(1, -1)];

        /**
         * Checks if all parts are connected to the core
         */
        public bool AreAllPartsConnected(EntityUid coreUid, HFRCoreComponent core)
        {
            if (!coreUid.IsValid() || !EntityManager.EntityExists(coreUid))
                return false;

            if (core.ConsoleUid == null || !EntityManager.EntityExists(core.ConsoleUid.Value) ||
                core.FuelInputUid == null || !EntityManager.EntityExists(core.FuelInputUid.Value) ||
                core.ModeratorInputUid == null || !EntityManager.EntityExists(core.ModeratorInputUid.Value) ||
                core.WasteOutputUid == null || !EntityManager.EntityExists(core.WasteOutputUid.Value))
                return false;

            if (core.CornerUids.Count != DiagonalOffsets.Length)
                return false;

            foreach (var cornerUid in core.CornerUids)
            {
                if (!EntityManager.EntityExists(cornerUid) ||
                    !EntityManager.TryGetComponent<HFRCornerComponent>(cornerUid, out var cornerComp) ||
                    cornerComp.CoreUid != coreUid)
                {
                    return false;
                }
            }

            return true;
        }

        /**
        * Toggles active status of all parts connected to the core
        */
        public void ToggleActiveState(EntityUid coreUid, HFRCoreComponent core, bool isActive)
        {
            core.IsActive = isActive;

            // Update core
            if (TryComp<AppearanceComponent>(coreUid, out var coreAppearance))
            {
                _appearanceSystem.SetData(coreUid, HFRVisuals.IsActive, isActive, coreAppearance);
            }
            if (isActive)
            {
                if (EntityManager.HasComponent<AnchorableComponent>(coreUid))
                    EntityManager.RemoveComponent<AnchorableComponent>(coreUid);
            }
            else
            {
                if (!EntityManager.HasComponent<AnchorableComponent>(coreUid))
                    // Only add AnchorableComponent if the entity is not terminating
                    if (EntityManager.TryGetComponent<MetaDataComponent>(coreUid, out var metadata) && metadata.EntityLifeStage < EntityLifeStage.Terminating)
                        EntityManager.AddComponent<AnchorableComponent>(coreUid);
            }

            // Update console
            if (core.ConsoleUid != null && EntityManager.TryGetComponent<HFRConsoleComponent>(core.ConsoleUid, out var consoleComp))
            {
                consoleComp.IsActive = isActive;
                if (TryComp<AppearanceComponent>(core.ConsoleUid.Value, out var consoleAppearance))
                {
                    _appearanceSystem.SetData(core.ConsoleUid.Value, HFRVisuals.IsActive, isActive, consoleAppearance);
                }
                if (TryComp<ApcPowerReceiverComponent>(core.ConsoleUid.Value, out var consolePower))
                {
                    consolePower.Load = isActive ? 250000f : (core.PowerLevel > 0 ? consolePower.Load : 350f);
                }
                if (isActive)
                {
                    if (EntityManager.HasComponent<AnchorableComponent>(core.ConsoleUid.Value))
                        EntityManager.RemoveComponent<AnchorableComponent>(core.ConsoleUid.Value);
                }
                else
                {
                    if (!EntityManager.HasComponent<AnchorableComponent>(core.ConsoleUid.Value))
                        EntityManager.AddComponent<AnchorableComponent>(core.ConsoleUid.Value);
                }
            }

            // Update fuel input
            if (core.FuelInputUid != null && EntityManager.TryGetComponent<HFRFuelInputComponent>(core.FuelInputUid, out var fuelComp))
            {
                fuelComp.IsActive = isActive;
                if (TryComp<AppearanceComponent>(core.FuelInputUid.Value, out var fuelAppearance))
                {
                    _appearanceSystem.SetData(core.FuelInputUid.Value, HFRVisuals.IsActive, isActive, fuelAppearance);
                }
                if (isActive)
                {
                    if (EntityManager.HasComponent<AnchorableComponent>(core.FuelInputUid.Value))
                        EntityManager.RemoveComponent<AnchorableComponent>(core.FuelInputUid.Value);
                }
                else
                {
                    if (!EntityManager.HasComponent<AnchorableComponent>(core.FuelInputUid.Value))
                        EntityManager.AddComponent<AnchorableComponent>(core.FuelInputUid.Value);
                }
            }

            // Update moderator input
            if (core.ModeratorInputUid != null && EntityManager.TryGetComponent<HFRModeratorInputComponent>(core.ModeratorInputUid, out var modComp))
            {
                modComp.IsActive = isActive;
                if (TryComp<AppearanceComponent>(core.ModeratorInputUid.Value, out var modAppearance))
                {
                    _appearanceSystem.SetData(core.ModeratorInputUid.Value, HFRVisuals.IsActive, isActive, modAppearance);
                }
                if (isActive)
                {
                    if (EntityManager.HasComponent<AnchorableComponent>(core.ModeratorInputUid.Value))
                        EntityManager.RemoveComponent<AnchorableComponent>(core.ModeratorInputUid.Value);
                }
                else
                {
                    if (!EntityManager.HasComponent<AnchorableComponent>(core.ModeratorInputUid.Value))
                        EntityManager.AddComponent<AnchorableComponent>(core.ModeratorInputUid.Value);
                }
            }

            // Update waste output
            if (core.WasteOutputUid != null && EntityManager.TryGetComponent<HFRWasteOutputComponent>(core.WasteOutputUid, out var wasteComp))
            {
                wasteComp.IsActive = isActive;
                if (TryComp<AppearanceComponent>(core.WasteOutputUid.Value, out var wasteAppearance))
                {
                    _appearanceSystem.SetData(core.WasteOutputUid.Value, HFRVisuals.IsActive, isActive, wasteAppearance);
                }
                if (isActive)
                {
                    if (EntityManager.HasComponent<AnchorableComponent>(core.WasteOutputUid.Value))
                        EntityManager.RemoveComponent<AnchorableComponent>(core.WasteOutputUid.Value);
                }
                else
                {
                    if (!EntityManager.HasComponent<AnchorableComponent>(core.WasteOutputUid.Value))
                        EntityManager.AddComponent<AnchorableComponent>(core.WasteOutputUid.Value);
                }
            }

            // Update corners
            foreach (var cornerUid in core.CornerUids)
            {
                if (EntityManager.TryGetComponent<HFRCornerComponent>(cornerUid, out var cornerComp))
                {
                    cornerComp.IsActive = isActive;
                    if (TryComp<AppearanceComponent>(cornerUid, out var cornerAppearance))
                    {
                        _appearanceSystem.SetData(cornerUid, HFRVisuals.IsActive, isActive, cornerAppearance);
                    }
                    if (isActive)
                    {
                        if (EntityManager.HasComponent<AnchorableComponent>(cornerUid))
                            EntityManager.RemoveComponent<AnchorableComponent>(cornerUid);
                    }
                    else
                    {
                        if (!EntityManager.HasComponent<AnchorableComponent>(cornerUid))
                            EntityManager.AddComponent<AnchorableComponent>(cornerUid);
                    }
                }
            }

            UpdateConsolePowerState(core);
        }

        public void UpdateConsolePowerState(HFRCoreComponent core)
        {
            if (core.ConsoleUid != null && EntityManager.TryGetComponent<HFRConsoleComponent>(core.ConsoleUid, out var consoleComp))
            {
                _hfrConsoleSystem.SetPowerState(core.ConsoleUid.Value, consoleComp);
            }
        }

        /**
         * Updates temperature and power level status
         */
        public void UpdateTemperatureStatus(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            core.FusionTemperatureArchived = core.FusionTemperature;
            core.FusionTemperature = core.InternalFusion?.Temperature ?? 293.15f;
            core.ModeratorTemperatureArchived = core.ModeratorTemperature;
            core.ModeratorTemperature = core.ModeratorInternal?.Temperature ?? 293.15f;
            core.CoolantTemperatureArchived = core.CoolantTemperature;
            core.CoolantTemperature = _nodeContainer.TryGetNode(coreUid, "pipe", out PipeNode? corePipe) ? corePipe?.Air.Temperature ?? 0f : 0f;
            core.OutputTemperatureArchived = core.OutputTemperature;
            core.OutputTemperature = core.WasteOutputUid != null && _nodeContainer.TryGetNode(core.WasteOutputUid.Value, "pipe", out PipeNode? wastePipe) ? wastePipe?.Air.Temperature ?? 0f : 0f;
            core.TemperaturePeriod = secondsPerTick;

            core.PowerLevel = core.FusionTemperature switch
            {
                <= 500 => 0,
                <= 1000 => 1,
                <= 10000 => 2,
                <= 100000 => 3,
                <= 1000000 => 4,
                <= 10000000 => 5,
                _ => 6
            };
        }

        /// <summary>
        /// Manages the looping ambient sound for the hypertorus, adjusting based on power level and critical state.
        /// </summary>
        private void HandleSoundLoop(EntityUid coreUid, HFRCoreComponent core)
        {
            if (!TryComp<AmbientSoundComponent>(coreUid, out var ambient))
                return;

            if (!(core.PowerLevel > 0) && !core.IsActive)
            {
                if (ambient.Enabled)
                    _ambient.SetAmbience(coreUid, false, ambient);
                return;
            }
            else
            {
                if (!ambient.Enabled)
                    _ambient.SetAmbience(coreUid, true, ambient);
            }

            var volume = (float)Math.Round(Math.Clamp((core.PowerLevel + 1) / 2f - 5, -5, 5));

            _ambient.SetVolume(coreUid, volume);

            SoundSpecifier targetSound = core.CriticalThresholdProximity >= 300
                ? core.MeltdownLoopSound
                : core.CalmLoopSound;

            if (core.CurrentSoundLoop != targetSound)
            {
                core.CurrentSoundLoop = targetSound;
                _ambient.SetSound(coreUid, core.CurrentSoundLoop, ambient);
            }
        }

        /**
         * Infrequently plays accent sounds
         */
        public void PlayAccent(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.LastAccentSound < _timing.CurTime && _random.Prob(0.2f))
            {
                var aggression = Math.Min((core.CriticalThresholdProximity / 800f) * (core.PowerLevel / 5f), 1f) * 100f;
                if (core.CriticalThresholdProximity >= 300)
                {
                    var soundIndex = _random.Next(1, 34);
                    _audioSystem.PlayPvs(
                        $"/Audio/_EE/Supermatter/accent/delam/{soundIndex}.ogg",
                        coreUid,
                        AudioParams.Default.WithVolume(Math.Max(20, aggression)).WithMaxDistance(40)
                    );
                }
                else
                {
                    var soundIndex = _random.Next(1, 34);
                    _audioSystem.PlayPvs(
                        $"/Audio/_EE/Supermatter/accent/normal/{soundIndex}.ogg",
                        coreUid,
                        AudioParams.Default.WithVolume(Math.Max(20, aggression)).WithMaxDistance(25)
                    );
                }
                var nextSound = Math.Round((100 - aggression) * 5) + 5;
                core.LastAccentSound = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(HypertorusFusionReactor.HypertorusAccentSoundMinCooldown, nextSound));
            }
        }

        /**
         * Getter for fusion fuel moles
         */
        public bool CheckFuel(HFRCoreComponent core)
        {
            if (string.IsNullOrEmpty(core.SelectedRecipeId) ||
                core.InternalFusion == null ||
                core.InternalFusion.TotalMoles <= 0 ||
                !_prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out var recipe))
                return false;

            return recipe.Requirements.All(gas => core.InternalFusion.GetMoles(Enum.Parse<Gas>(gas)) >= HypertorusFusionReactor.FusionMoleThreshold);
        }

        /**
         * Checks if the gases in the input are the ones needed by the recipe
         */
        public bool CheckGasRequirements(HFRCoreComponent core, FusionRecipePrototype recipe)
        {
            if (core.FuelInputUid == null || !_nodeContainer.TryGetNode(core.FuelInputUid.Value, "pipe", out PipeNode? fuelPipe))
                return false;

            var contents = fuelPipe.Air;
            foreach (var gas in recipe.Requirements)
            {
                if (contents.GetMoles(Enum.Parse<Gas>(gas)) <= 0)
                    return false;
            }

            return true;
        }

        /**
         * Removes the gases from the internal gasmix when the recipe is changed
         */
        public void DumpGases(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.WasteOutputUid != null && _nodeContainer.TryGetNode(core.WasteOutputUid.Value, "pipe", out PipeNode? wastePipe) && core.InternalFusion != null)
            {
                var remove = core.InternalFusion;
                core.InternalFusion = new GasMixture { Volume = 5000 };
                _atmosSystem.Merge(wastePipe.Air, remove);
            }
        }

        /**
         * Check the integrity level and returns the status of the machine
         */
        public int GetStatus(HFRCoreComponent core)
        {
            var integrity = GetIntegrityPercent(core);
            if (integrity < HypertorusFusionReactor.HypertorusMeltingPercent)
                return 5;
            if (integrity < HypertorusFusionReactor.HypertorusEmergencyPercent)
                return 4;
            if (integrity < HypertorusFusionReactor.HypertorusDangerPercent)
                return 3;
            if (integrity < HypertorusFusionReactor.HypertorusWarningPercent)
                return 2;
            if (core.PowerLevel > 0)
                return 1;
            return 0;
        }

        /**
         * Play a sound from the machine, the type depends on the status of the hfr
         */
        public void Alarm(EntityUid coreUid, HFRCoreComponent core)
        {
            switch (GetStatus(core))
            {
                case 5:
                    _audioSystem.PlayPvs("/Audio/_EE/Supermatter/status/bloblarm.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(40));
                    break;
                case 4:
                    _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/engine_alert1.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(30));
                    break;
                case 3:
                    _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/engine_alert2.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(30));
                    break;
                case 2:
                    _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/terminal_alert.ogg", coreUid, AudioParams.Default.WithVolume(75));
                    break;
            }
        }

        /**
         * Getter for the machine integrity
         */
        public float GetIntegrityPercent(HFRCoreComponent core)
        {
            var integrity = core.CriticalThresholdProximity / core.MeltingPoint;
            integrity = MathF.Round(100f - integrity * 100f, 2);
            return Math.Max(integrity, 0f);
        }

        /**
        * Get how charged the area's APC is
        */
        public float GetAreaCellPercent(EntityUid coreUid)
        {
            // TODO: Figure this out
            return 100f;
        }

        /**
         * Sends the given message to local chat and a radio channel
         * @param global If true, sends the message to the common radio channel
         */
        public void SendHypertorusAnnouncement(EntityUid coreUid, HFRCoreComponent core, string message, bool global = false)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var channel = global ? "Common" : "Engineering";
            core.LastWarning = _timing.CurTime;
            core.LastWarningThresholdProximity = core.CriticalThresholdProximity;

            // Send to in-game IC chat and radio
            _chatSystem.TrySendInGameICMessage(coreUid, message, InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
            _radioSystem.SendRadioMessage(coreUid, message, channel, coreUid);
        }

        /**
        * Broadcast messages into engi and common radio
        */
        public void CheckAlert(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.FinalCountdown || core.CriticalThresholdProximity > core.MeltingPoint)
            {
                Countdown(coreUid, core);
                return;
            }

            // Dumb but quick way to clear emergency status
            core.HasReachedEmergency = core.HasReachedEmergency ? core.CriticalThresholdProximity > core.EmergencyPoint : core.HasReachedEmergency;

            if (core.CriticalThresholdProximity < core.WarningPoint)
                return;

            var integrity = GetIntegrityPercent(core).ToString("0.00");
            string message = "";
            bool global = false;
            bool warningDelayOver = _timing.CurTime > core.LastWarning + TimeSpan.FromSeconds(HypertorusFusionReactor.WarningTimeDelay);

            if (core.CriticalThresholdProximity > core.EmergencyPoint && !core.HasReachedEmergency)
            {
                message = $"{core.EmergencyAlert} Integrity: {integrity}%";
                global = true;
                // TODO: Add admin log here
                core.HasReachedEmergency = true;
            }
            else if (core.HasReachedEmergency && core.CriticalThresholdProximity > core.LastWarningThresholdProximity + 40f && warningDelayOver)
            {
                message = $"{core.EmergencyAlert} Integrity: {integrity}%";
                global = true;
            }
            else if (core.CriticalThresholdProximity >= core.CriticalThresholdProximityArchived && core.CriticalThresholdProximity > core.LastWarningThresholdProximity + 40f && warningDelayOver)
            {
                message = $"{core.WarningAlert} Integrity: {integrity}%";
            }
            else if (core.CriticalThresholdProximity < core.LastWarningThresholdProximity - 30f && warningDelayOver)
            {
                message = $"{core.SafeAlert} Integrity: {integrity}%";
            }

            if (message != "")
            {
                Alarm(coreUid, core);
                SendRadioExplanation(coreUid, core, message, global);
            }
        }

        /**
        * Modifies the message with additional details and sends it as a single announcement
        */
        public void SendRadioExplanation(EntityUid coreUid, HFRCoreComponent core, string baseMessage, bool global = false)
        {
            var messageBuilder = new StringBuilder("");
            messageBuilder.AppendLine(baseMessage);

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.Emped))
            {
                var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ".ToCharArray();
                var message = new string(Enumerable.Repeat(characters, _random.Next(50, 70)).Select(s => s[_random.Next(s.Length)]).ToArray());
                SendHypertorusAnnouncement(coreUid, core, message, global);
                return;
            }

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.HighPowerDamage))
                messageBuilder.AppendLine("Shield destabilizing due to excessive power!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.IronContentDamage))
                messageBuilder.AppendLine("Iron shards are damaging the internal core shielding!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.HighFuelMixMole))
                messageBuilder.AppendLine("Fuel mix moles reaching critical levels!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.IronContentIncrease))
                messageBuilder.AppendLine("Iron amount inside the core is increasing!");

            SendHypertorusAnnouncement(coreUid, core, messageBuilder.ToString(), global);
        }

        /**
        * Called when the damage has reached critical levels, starts the countdown before destruction, calls Meltdown()
        */
        public void Countdown(EntityUid coreUid, HFRCoreComponent core)
        {
            FusionRecipePrototype? recipe = null;
            bool recipeValid = !string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe);

            // Initialize countdown
            if (!core.FinalCountdown)
            {
                Alarm(coreUid, core);
                core.FinalCountdown = true;
                core.CountdownStartTime = _timing.CurTime;
                core.LastCountdownUpdate = _timing.CurTime;

                var critical = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.CriticalMeltdown);

                if (critical)
                {
                    var criticalMessage = "WARNING - The explosion will likely cover a large part of the station and the coming EMP will wipe out most electronics. Evacuate immediately or attempt to shut down the reactor.";
                    SendHypertorusAnnouncement(coreUid, core, criticalMessage, true);
                }

                var initialMessage = $"{core.EmergencyAlert} The Hypertorus fusion reactor has reached critical integrity failure. Emergency magnetic dampeners online.";
                SendHypertorusAnnouncement(coreUid, core, initialMessage, true);
            }

            // Check if reactor is safe
            if (core.CriticalThresholdProximity < core.MeltingPoint)
            {
                var safeMessage = $"{core.SafeAlert} Failsafe has been disengaged.";
                SendHypertorusAnnouncement(coreUid, core, safeMessage, true);
                core.FinalCountdown = false;
                core.CountdownStartTime = TimeSpan.Zero;
                return;
            }

            // Calculate elapsed time since countdown started
            var elapsedTime = _timing.CurTime - core.CountdownStartTime;
            var totalCountdownTime = TimeSpan.FromSeconds(HypertorusFusionReactor.HypertorusCountdownTime);
            var remainingTime = totalCountdownTime - elapsedTime;

            // If countdown is complete, trigger meltdown
            if (remainingTime <= TimeSpan.Zero)
            {
                Meltdown(coreUid, core);
                return;
            }

            // Only send messages every 1 second
            if (_timing.CurTime < core.LastCountdownUpdate + TimeSpan.FromSeconds(1))
                return;

            // Determine message based on remaining time
            string message;
            if (remainingTime > TimeSpan.FromSeconds(34)) // Set to 34 so 5 second timer can take over at 30
            {
                // Send message every 30 seconds for remaining time > 30 seconds
                var secondsRemaining = (int)Math.Ceiling(remainingTime.TotalSeconds);
                if (secondsRemaining % 30 != 0 || secondsRemaining == 0)
                {
                    core.LastCountdownUpdate = _timing.CurTime;
                    return;
                }

                message = $"{DisplayTimeText(secondsRemaining, true)} remain before total integrity failure.";
            }
            else if (remainingTime > TimeSpan.FromSeconds(5))
            {
                // Send message every 5 seconds for remaining time > 5 seconds
                var secondsRemaining = (int)Math.Ceiling(remainingTime.TotalSeconds);
                if (secondsRemaining % 5 != 0)
                {
                    core.LastCountdownUpdate = _timing.CurTime;
                    return;
                }

                if (secondsRemaining == 10 && recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.CriticalMeltdown))
                    _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/HFR_critical_explosion.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(40));

                message = $"{DisplayTimeText(secondsRemaining, true)} remain before total integrity failure.";
            }
            else
            {
                // Send message every call
                message = $"{(int)Math.Ceiling(remainingTime.TotalSeconds)}...";

                if (TryComp<SpeechComponent>(coreUid, out var speech))
                    speech.SoundCooldownTime = 0.9f;
            }

            SendHypertorusAnnouncement(coreUid, core, message, true);
            core.LastCountdownUpdate = _timing.CurTime;
        }

        /**
        * Create the explosion, gas emission, radiation pulse, and EMP before deleting the machine core.
        */
        public void Meltdown(EntityUid coreUid, HFRCoreComponent core)
        {
            // Return immediately if TimedDespawnComponent is already present
            if (_entityManager.HasComponent<TimedDespawnComponent>(coreUid))
                return;

            FusionRecipePrototype? recipe = null;
            bool recipeValid = !string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe);
            var critical = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.CriticalMeltdown);

            int flashExplosion = 0, lightImpactExplosion = 0, heavyImpactExplosion = 0, devastatingExplosion = 0;
            int empLightSize = 0, empHeavySize = 0, radPulseSize = 0;
            int gasPockets = 0, gasSpread = 0;
            bool emPulse = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.EMP);
            bool radPulse = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.RadiationPulse);

            if (recipeValid && recipe != null)
            {
                // Explosion scaling
                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.BaseExplosion))
                {
                    flashExplosion = core.PowerLevel * 3;
                    lightImpactExplosion = core.PowerLevel * 2;
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MediumExplosion))
                {
                    flashExplosion = core.PowerLevel * 6;
                    lightImpactExplosion = core.PowerLevel * 5;
                    heavyImpactExplosion = (int)(core.PowerLevel * 0.5f);
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.DevastatingExplosion))
                {
                    flashExplosion = core.PowerLevel * 8;
                    lightImpactExplosion = core.PowerLevel * 7;
                    heavyImpactExplosion = core.PowerLevel * 2;
                    devastatingExplosion = core.PowerLevel;
                }

                // Spread and pulse scaling
                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MinimumSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 3;
                        empHeavySize = core.PowerLevel * 1;
                    }
                    if (radPulse)
                    {
                        radPulseSize = 2 * core.PowerLevel + 8;
                    }
                    gasPockets = 5;
                    gasSpread = core.PowerLevel * 2;
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MediumSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 5;
                        empHeavySize = core.PowerLevel * 3;
                    }
                    if (radPulse)
                    {
                        radPulseSize = core.PowerLevel + 24;
                    }
                    gasPockets = 7;
                    gasSpread = core.PowerLevel * 4;
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.BigSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 7;
                        empHeavySize = core.PowerLevel * 5;
                    }
                    if (radPulse)
                    {
                        radPulseSize = core.PowerLevel + 34;
                    }
                    gasPockets = 10;
                    gasSpread = core.PowerLevel * 6;
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MassiveSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 9;
                        empHeavySize = core.PowerLevel * 7;
                    }
                    if (radPulse)
                    {
                        radPulseSize = core.PowerLevel + 44;
                    }
                    gasPockets = 15;
                    gasSpread = core.PowerLevel * 8;
                }
            }

            // Gas emission - currently does not double for critical explosions as I feel this scales horribly. Someone please make this more workable.
            var transform = _entityManager.GetComponent<TransformComponent>(coreUid);
            var coords = transform.Coordinates;
            if (_transformSystem.IsValid(coords) && _mapManager.TryFindGridAt(_transformSystem.ToMapCoordinates(coords), out var gridUid, out var gridComp))
            {
                var gridEntity = new Entity<GridAtmosphereComponent?, GasTileOverlayComponent?>(
                    gridUid,
                    _entityManager.GetComponent<GridAtmosphereComponent>(gridUid),
                    _entityManager.GetComponent<GasTileOverlayComponent>(gridUid)
                );
                var mapUid = _mapManager.GetMapEntityId(transform.MapID);
                var mapEntity = new Entity<MapAtmosphereComponent?>(
                    mapUid,
                    _entityManager.TryGetComponent<MapAtmosphereComponent>(mapUid, out var mapAtmos) ? mapAtmos : null
                );

                // Get tiles in circular range for 20% gas distribution
                var centerTile = _mapSystem.CoordinatesToTile(gridUid, gridComp, coords);
                var tiles = new List<TileRef>();
                var tileSize = gridComp.TileSize;
                var centerPos = _mapSystem.TileCenterToVector((gridUid, gridComp), centerTile);
                var circle = new Circle(centerPos, gasSpread * tileSize);

                foreach (var tileRef in _mapSystem.GetLocalTilesIntersecting(gridUid, gridComp, circle, ignoreEmpty: true))
                {
                    if (!_atmosSystem.IsTileAirBlocked(gridUid, tileRef.GridIndices))
                    {
                        tiles.Add(tileRef);
                    }
                }

                // Distribute fusion gas (20% to random tiles)
                if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0)
                {
                    var fusionToRemove = core.InternalFusion.RemoveRatio(0.2f);
                    if (fusionToRemove != null && tiles.Count > 0)
                    {
                        var gasPerPocket = 1f / gasPockets;
                        var shuffledTiles = tiles.ToList();
                        for (int i = shuffledTiles.Count - 1; i > 0; i--)
                        {
                            var j = _random.Next(i + 1);
                            (shuffledTiles[i], shuffledTiles[j]) = (shuffledTiles[j], shuffledTiles[i]);
                        }

                        for (int i = 0; i < Math.Min(gasPockets, shuffledTiles.Count); i++)
                        {
                            var tileRef = shuffledTiles[i];
                            var mixture = _atmosSystem.GetTileMixture(gridEntity, mapEntity, tileRef.GridIndices, excite: true);
                            if (mixture != null)
                            {
                                var pocket = fusionToRemove.RemoveRatio(gasPerPocket);
                                if (pocket != null)
                                {
                                    _atmosSystem.Merge(mixture, pocket);
                                }
                            }
                        }
                    }
                }

                // Distribute moderator gas (20% to random tiles)
                if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0)
                {
                    var moderatorToRemove = core.ModeratorInternal.RemoveRatio(0.2f);
                    if (moderatorToRemove != null && tiles.Count > 0)
                    {
                        var gasPerPocket = 1f / gasPockets;
                        var shuffledTiles = tiles.ToList();
                        for (int i = shuffledTiles.Count - 1; i > 0; i--)
                        {
                            var j = _random.Next(i + 1);
                            (shuffledTiles[i], shuffledTiles[j]) = (shuffledTiles[j], shuffledTiles[i]);
                        }

                        for (int i = 0; i < Math.Min(gasPockets, shuffledTiles.Count); i++)
                        {
                            var tileRef = shuffledTiles[i];
                            var mixture = _atmosSystem.GetTileMixture(gridEntity, mapEntity, tileRef.GridIndices, excite: true);
                            if (mixture != null)
                            {
                                var pocket = moderatorToRemove.RemoveRatio(gasPerPocket);
                                if (pocket != null)
                                {
                                    _atmosSystem.Merge(mixture, pocket);
                                }
                            }
                        }
                    }
                }

                // Dump remaining gas into 3x3 area around core tile
                var coreAreaTiles = new List<TileRef>();
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        var tileIndices = centerTile + new Vector2i(x, y);
                        if (_mapSystem.TryGetTileRef(gridUid, gridComp, tileIndices, out var tileRef) &&
                            !tileRef.Tile.IsEmpty &&
                            !_atmosSystem.IsTileAirBlocked(gridUid, tileIndices))
                        {
                            coreAreaTiles.Add(tileRef);
                        }
                    }
                }

                if (coreAreaTiles.Count > 0)
                {
                    var gasPerTile = 1f / coreAreaTiles.Count;

                    // Dump remaining fusion gas into 3x3 area
                    if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0)
                    {
                        foreach (var tileRef in coreAreaTiles)
                        {
                            var mixture = _atmosSystem.GetTileMixture(gridEntity, mapEntity, tileRef.GridIndices, excite: true);
                            if (mixture != null)
                            {
                                var pocket = core.InternalFusion.RemoveRatio(gasPerTile);
                                if (pocket != null)
                                {
                                    _atmosSystem.Merge(mixture, pocket);
                                }
                            }
                        }
                    }

                    // Dump remaining moderator gas into 3x3 area
                    if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0)
                    {
                        foreach (var tileRef in coreAreaTiles)
                        {
                            var mixture = _atmosSystem.GetTileMixture(gridEntity, mapEntity, tileRef.GridIndices, excite: true);
                            if (mixture != null)
                            {
                                var pocket = core.ModeratorInternal.RemoveRatio(gasPerTile);
                                if (pocket != null)
                                {
                                    _atmosSystem.Merge(mixture, pocket);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // No valid tiles in 3x3 area, dump into core tile
                    var coreTileMixture = _atmosSystem.GetContainingMixture(coreUid, excite: true);
                    if (coreTileMixture != null)
                    {
                        if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0)
                        {
                            _atmosSystem.Merge(coreTileMixture, core.InternalFusion);
                        }
                        if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0)
                        {
                            _atmosSystem.Merge(coreTileMixture, core.ModeratorInternal);
                        }
                    }
                }

                _gasOverlaySystem.UpdateSessions();
            }
            else
            {
                // No valid grid, just dump all gas into core tile
                var coreTileMixture = _atmosSystem.GetContainingMixture(coreUid, excite: false);
                if (coreTileMixture != null)
                {
                    if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0)
                    {
                        _atmosSystem.Merge(coreTileMixture, core.InternalFusion);
                    }
                    if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0)
                    {
                        _atmosSystem.Merge(coreTileMixture, core.ModeratorInternal);
                    }
                }
                _gasOverlaySystem.UpdateSessions();
            }

            // Explosion
            var totalIntensity = (devastatingExplosion + heavyImpactExplosion + lightImpactExplosion + flashExplosion) * 100f;
            totalIntensity = critical ? totalIntensity * 2 : totalIntensity;
            if (totalIntensity > 0)
            {
                _explosionSystem.QueueExplosion(
                    coreUid,
                    ExplosionSystem.DefaultExplosionPrototypeId,
                    totalIntensity,
                    slope: 1.25f,
                    maxTileIntensity: 200,
                    tileBreakScale: 0.1f,
                    maxTileBreak: 1,
                    canCreateVacuum: true,
                    user: null,
                    addLog: true
                );
            }

            // Radiation Pulse - barely worth doing but thrown in for now. Someone please replace this with something that works. 
            if (radPulse && radPulseSize > 0)
            {
                if (_entityManager.TryGetComponent<RadiationSourceComponent>(coreUid, out var radSource))
                {
                    radSource.Intensity = radPulseSize * 4f;
                    radSource.Slope = Math.Clamp(radSource.Intensity / 15f, 0.5f, 2f); // no idea
                }
            }

            // EMP Pulse
            if (emPulse)
            {
                var empCoords = _transformSystem.GetMapCoordinates(coreUid);
                var empRange = critical ? empLightSize * 4 : empLightSize;
                var energyConsumption = empHeavySize * 1000f;
                var duration = 30f;
                _empSystem.EmpPulse(
                    empCoords,
                    range: empRange,
                    energyConsumption: energyConsumption,
                    duration: duration
                );
            }

            ToggleActiveState(coreUid, core, false);
            var despawn = _entityManager.EnsureComponent<TimedDespawnComponent>(coreUid);
            despawn.Lifetime = 1.1f;
        }

        /**
        * Formats time for countdown announcements
        */
        private string DisplayTimeText(int time, bool isSeconds)
        {
            if (isSeconds)
            {
                return $"{time} second{(time != 1 ? "s" : "")}";
            }
            return string.Empty;
        }

        /**
         * Checks if any connected parts are cracked
         */
        public bool CheckCrackedParts(EntityUid coreUid, HFRCoreComponent core)
        {
            // Check fuel input
            if (core.FuelInputUid != null && _entityManager.TryGetComponent<HFRFuelInputComponent>(core.FuelInputUid.Value, out var fuelComp) && fuelComp.Cracked)
                return true;

            // Check moderator input
            if (core.ModeratorInputUid != null && _entityManager.TryGetComponent<HFRModeratorInputComponent>(core.ModeratorInputUid.Value, out var modComp) && modComp.Cracked)
                return true;

            // Check waste output
            if (core.WasteOutputUid != null && _entityManager.TryGetComponent<HFRWasteOutputComponent>(core.WasteOutputUid.Value, out var wasteComp) && wasteComp.Cracked)
                return true;

            // Check corners
            foreach (var cornerUid in core.CornerUids)
            {
                if (_entityManager.TryGetComponent<HFRCornerComponent>(cornerUid, out var cornerComp) && cornerComp.Cracked)
                    return true;
            }

            return false;
        }

        /**
         * Randomly selects a part and marks it as cracked
         */
        public EntityUid? CreateCrack(EntityUid coreUid, HFRCoreComponent core)
        {
            var parts = new List<EntityUid>();

            // Collect all valid parts
            if (core.FuelInputUid != null && _entityManager.EntityExists(core.FuelInputUid.Value))
                parts.Add(core.FuelInputUid.Value);
            if (core.ModeratorInputUid != null && _entityManager.EntityExists(core.ModeratorInputUid.Value))
                parts.Add(core.ModeratorInputUid.Value);
            if (core.WasteOutputUid != null && _entityManager.EntityExists(core.WasteOutputUid.Value))
                parts.Add(core.WasteOutputUid.Value);
            parts.AddRange(core.CornerUids.Where(uid => _entityManager.EntityExists(uid)));

            if (parts.Count == 0)
                return null;

            // Pick a random part
            var selectedPart = _random.Pick(parts);

            // Mark the part as cracked and update appearance
            if (_entityManager.TryGetComponent<HFRFuelInputComponent>(selectedPart, out var fuelComp))
            {
                fuelComp.Cracked = true;
                if (_entityManager.TryGetComponent<AppearanceComponent>(selectedPart, out var appearance))
                    _appearanceSystem.SetData(selectedPart, HFRVisuals.Cracked, true, appearance);
            }
            else if (_entityManager.TryGetComponent<HFRModeratorInputComponent>(selectedPart, out var modComp))
            {
                modComp.Cracked = true;
                if (_entityManager.TryGetComponent<AppearanceComponent>(selectedPart, out var appearance))
                    _appearanceSystem.SetData(selectedPart, HFRVisuals.Cracked, true, appearance);
            }
            else if (_entityManager.TryGetComponent<HFRWasteOutputComponent>(selectedPart, out var wasteComp))
            {
                wasteComp.Cracked = true;
                if (_entityManager.TryGetComponent<AppearanceComponent>(selectedPart, out var appearance))
                    _appearanceSystem.SetData(selectedPart, HFRVisuals.Cracked, true, appearance);
            }
            else if (_entityManager.TryGetComponent<HFRCornerComponent>(selectedPart, out var cornerComp))
            {
                cornerComp.Cracked = true;
                if (_entityManager.TryGetComponent<AppearanceComponent>(selectedPart, out var appearance))
                    _appearanceSystem.SetData(selectedPart, HFRVisuals.Cracked, true, appearance);
            }

            return selectedPart;
        }

        /**
         * Spills gases from the target gas mixture into the specified entity's tile
         */
        public void SpillGases(EntityUid origin, GasMixture targetMix, float ratio)
        {
            if (targetMix.TotalMoles <= 0 || ratio <= 0)
                return;

            var tileMixture = _atmosSystem.GetContainingMixture(origin, false, false);
            if (tileMixture == null)
                return;

            var removedMix = targetMix.RemoveRatio(ratio);
            _atmosSystem.Merge(tileMixture, removedMix);
        }

        /**
         * Checks for gas leaks from cracked parts and handles initial rupture explosions
         */
        public void CheckSpill(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            // Declare pressure once at the top
            var pressure = core.ModeratorInternal?.Pressure ?? 0f;

            // Check for existing cracked parts
            if (CheckCrackedParts(coreUid, core))
            {
                var crackedPart = GetCrackedPart(core);
                if (crackedPart == null)
                    return;

                float leakRate;

                if (pressure < HypertorusFusionReactor.HypertorusMediumSpillPressure)
                {
                    if (!_random.Prob(HypertorusFusionReactor.HypertorusWeakSpillChance))
                        return;
                    leakRate = HypertorusFusionReactor.HypertorusWeakSpillRate;
                }
                else if (pressure < HypertorusFusionReactor.HypertorusStrongSpillPressure)
                {
                    leakRate = HypertorusFusionReactor.HypertorusMediumSpillRate;
                }
                else
                {
                    leakRate = HypertorusFusionReactor.HypertorusStrongSpillRate;
                }

                if (core.ModeratorInternal != null)
                    SpillGases(crackedPart.Value, core.ModeratorInternal, 1f - MathF.Pow(1f - leakRate, secondsPerTick));

                return;
            }

            // Check for conditions to create a new crack
            if (core.ModeratorInternal?.TotalMoles < HypertorusFusionReactor.HypertorusHypercriticalMoles)
                return;

            var newCrackedPart = CreateCrack(coreUid, core);
            if (newCrackedPart == null)
                return;

            if (pressure < HypertorusFusionReactor.HypertorusMediumSpillPressure)
                return;

            if (pressure < HypertorusFusionReactor.HypertorusStrongSpillPressure)
            {
                // Medium explosion on initial rupture
                _explosionSystem.QueueExplosion(
                    newCrackedPart.Value,
                    "Default",
                    totalIntensity: 40f,
                    slope: 10f,
                    maxTileIntensity: 20f,
                    tileBreakScale: 1f,
                    maxTileBreak: 1,
                    canCreateVacuum: false
                );
                if (core.ModeratorInternal != null)
                    SpillGases(newCrackedPart.Value, core.ModeratorInternal, HypertorusFusionReactor.HypertorusMediumSpillInitial);
                return;
            }

            // Strong explosion on initial rupture
            _explosionSystem.QueueExplosion(
                newCrackedPart.Value,
                "Default",
                totalIntensity: 90f,
                slope: 10f,
                maxTileIntensity: 30f,
                tileBreakScale: 1f,
                maxTileBreak: 2,
                canCreateVacuum: false
            );
            if (core.ModeratorInternal != null)
                SpillGases(newCrackedPart.Value, core.ModeratorInternal, HypertorusFusionReactor.HypertorusStrongSpillInitial);
        }

        /**
         * Helper method to get a cracked part
         */
        private EntityUid? GetCrackedPart(HFRCoreComponent core)
        {
            if (core.FuelInputUid != null && _entityManager.TryGetComponent<HFRFuelInputComponent>(core.FuelInputUid.Value, out var fuelComp) && fuelComp.Cracked)
                return core.FuelInputUid.Value;
            if (core.ModeratorInputUid != null && _entityManager.TryGetComponent<HFRModeratorInputComponent>(core.ModeratorInputUid.Value, out var modComp) && modComp.Cracked)
                return core.ModeratorInputUid.Value;
            if (core.WasteOutputUid != null && _entityManager.TryGetComponent<HFRWasteOutputComponent>(core.WasteOutputUid.Value, out var wasteComp) && wasteComp.Cracked)
                return core.WasteOutputUid.Value;
            foreach (var cornerUid in core.CornerUids)
            {
                if (_entityManager.TryGetComponent<HFRCornerComponent>(cornerUid, out var cornerComp) && cornerComp.Cracked)
                    return cornerUid;
            }
            return null;
        }
    }
}