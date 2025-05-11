using Content.Server._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Robust.Shared.Map;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer;
using Robust.Shared.Maths;
using Content.Shared._Funkystation.Atmos.HFR;
using Content.Server.Power.EntitySystems;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Audio;
using Content.Server.Administration.Logs;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.Chat;
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Audio.Systems;
using Content.Shared._Funkystation.Atmos.Prototypes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Server.Atmos.EntitySystems;
using Content.Server._Funkystation.Atmos.HFR.Systems;
using Content.Shared.Speech;

using Content.Server._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Robust.Shared.Map;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer;
using Robust.Shared.Maths;
using Content.Shared._Funkystation.Atmos.HFR;
using Content.Server.Power.EntitySystems;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Audio;
using Content.Server.Administration.Logs;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.Chat;
using Content.Shared.Audio;
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared._Funkystation.Atmos.Prototypes;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Robust.Shared.Prototypes;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Emp;
using Content.Server.Emp;
using Content.Server.DoAfter;
using Content.Server.Forensics;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Server.Zombies;
using Content.Shared.Alert;
using Content.Shared.Changeling;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Store.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Serialization.Manager;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Flash;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Fluids;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared.Camera;
using Robust.Shared.Timing;
using Content.Shared.Damage.Components;
using Content.Server.Gravity;
using Content.Shared.Mobs.Components;
using Content.Server.Stunnable;
using Content.Shared.Jittering;
using Content.Server.Explosion.EntitySystems;
using System.Linq;
using Content.Shared.Forensics.Components;
using Content.Server._Funkystation.Atmos.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Linq;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Construction.Components;
using Content.Server._Funkystation.Atmos.HFR.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server._Funkystation.Atmos.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Linq;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Construction.Components;
using Content.Server._Funkystation.Atmos.HFR.Systems;
using Content.Server._Funkystation.Atmos.Systems;
using Content.Server.Lightning;
using Content.Server.Power.EntitySystems;
using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Robust.Shared.Physics.Components;

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Examine;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Traits.Assorted;
using Content.Shared._EE.CCVars;
using Content.Shared._EE.Supermatter.Components;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.DeviceLinking;
using System.Linq;
using System.Numerics;
using System.Text;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Singularity.Components;
using Content.Server.Sound.Components;
using Content.Shared._EE.CCVars;
using Content.Shared._EE.Supermatter.Components;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Radiation.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Speech;
using Content.Shared.Traits.Assorted;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Vector4 = Robust.Shared.Maths.Vector4;
using Content.Shared.DeviceLinking;

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
                    _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/bloblarm.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(40));
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
            // TODO: I will have to figure this one out later
            return 0f;
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

            // Send to in-game IC chat and radio
            _chatSystem.TrySendInGameICMessage(coreUid, message, InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
            _radioSystem.SendRadioMessage(coreUid, message, channel, coreUid);
        }

        /**
         * Broadcast messages into engi and common radio
         */
        public void CheckAlert(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.CriticalThresholdProximity < core.WarningPoint)
                return;

            if (_timing.CurTime < core.LastWarning + TimeSpan.FromSeconds(HypertorusFusionReactor.WarningTimeDelay))
                return;

            Alarm(coreUid, core);

            var integrity = GetIntegrityPercent(core).ToString("0.00");
            string message;
            bool global = false;

            if (core.CriticalThresholdProximity > core.EmergencyPoint)
            {
                message = $"{core.EmergencyAlert} Integrity: {integrity}%";
                global = true;
                if (!core.HasReachedEmergency)
                {
                    // TODO: Add admin log here
                    core.HasReachedEmergency = true;
                }
                SendHypertorusAnnouncement(coreUid, core, message, global);
                SendRadioExplanation(coreUid, core);
            }
            else if (core.CriticalThresholdProximity >= core.CriticalThresholdProximityArchived)
            {
                message = $"{core.WarningAlert} Integrity: {integrity}%";
                SendHypertorusAnnouncement(coreUid, core, message, global);
                core.LastWarning = _timing.CurTime - TimeSpan.FromSeconds(HypertorusFusionReactor.WarningTimeDelay * 5);
                SendRadioExplanation(coreUid, core);
            }
            else
            {
                message = $"{core.SafeAlert} Integrity: {integrity}%";
                SendHypertorusAnnouncement(coreUid, core, message, global);
            }

            if (core.CriticalThresholdProximity > core.MeltingPoint)
                Countdown(coreUid, core);
        }

        /**
         * Called to explain in radio what the issues are with the HFR
         */
        public void SendRadioExplanation(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.Emped))
            {
                var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ".ToCharArray();
                var message = new string(Enumerable.Repeat(characters, _random.Next(50, 70)).Select(s => s[_random.Next(s.Length)]).ToArray());
                SendHypertorusAnnouncement(coreUid, core, message);
                return;
            }

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.HighPowerDamage))
                SendHypertorusAnnouncement(coreUid, core, "Warning! Shield destabilizing due to excessive power!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.IronContentDamage))
                SendHypertorusAnnouncement(coreUid, core, "Warning! Iron shards are damaging the internal core shielding!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.HighFuelMixMole))
                SendHypertorusAnnouncement(coreUid, core, "Warning! Fuel mix moles reaching critical levels!");

            if (core.StatusFlags.HasFlag(HypertorusStatusFlags.IronContentIncrease))
                SendHypertorusAnnouncement(coreUid, core, "Warning! Iron amount inside the core is increasing!");
        }

        /**
         * Called when the damage has reached critical levels, start the countdown before the destruction, calls meltdown()
         */
        public void Countdown(EntityUid coreUid, HFRCoreComponent core)
        {
            if (core.FinalCountdown)
                return;

            core.FinalCountdown = true;
            core.LastCountdownUpdate = _timing.CurTime;

            FusionRecipePrototype? recipe = null;
            bool recipeValid = !string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe);
            var critical = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.CriticalMeltdown);

            if (critical)
            {
                var message = "WARNING - The explosion will likely cover a large part of the station and the coming EMP will wipe out most electronics. Evacuate immediately or attempt to shut down the reactor.";
                SendHypertorusAnnouncement(coreUid, core, message, true);
            }

            var initialMessage = $"{core.EmergencyAlert} The Hypertorus fusion reactor has reached critical integrity failure. Emergency magnetic dampeners online.";
            SendHypertorusAnnouncement(coreUid, core, initialMessage, true);

            for (int i = (int)HypertorusFusionReactor.HypertorusCountdownTime; i >= 0; i -= 10)
            {
                if (core.CriticalThresholdProximity < core.MeltingPoint)
                {
                    var safeMessage = $"{core.SafeAlert} Failsafe has been disengaged.";
                    SendHypertorusAnnouncement(coreUid, core, safeMessage, true);
                    core.FinalCountdown = false;
                    return;
                }

                // Check if at least 1 second has passed since the last update
                if (_timing.CurTime < core.LastCountdownUpdate + TimeSpan.FromSeconds(1))
                    continue;

                if (i % 50 != 0 && i > 50)
                {
                    core.LastCountdownUpdate = _timing.CurTime;
                    continue;
                }

                string message;
                if (i > 50)
                {
                    if (i == 100 && critical)
                        _audioSystem.PlayPvs("/Audio/_Funkystation/Hypertorus/HFR_critical_explosion.ogg", coreUid, AudioParams.Default.WithVolume(100).WithMaxDistance(40));

                    message = $"{DisplayTimeText(i, true)} remain before total integrity failure.";
                }
                else
                {
                    message = $"{i * 0.1:F1}...";

                    // Adjust speech cooldown for countdown
                    if (TryComp<SpeechComponent>(coreUid, out var speech))
                        speech.SoundCooldownTime = 0.9f;
                }

                SendHypertorusAnnouncement(coreUid, core, message, true);
                core.LastCountdownUpdate = _timing.CurTime;
            }

            Meltdown(coreUid, core);
        }

        /**
        * Create the explosion + the gas emission before deleting the machine core.
        */
        public void Meltdown(EntityUid coreUid, HFRCoreComponent core)
        {
            FusionRecipePrototype? recipe = null;
            bool recipeValid = !string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe);
            var critical = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.CriticalMeltdown);

            int flashExplosion = 0, lightImpactExplosion = 0, heavyImpactExplosion = 0, devastatingExplosion = 0;
            int empLightSize = 0, empHeavySize = 0;
            bool emPulse = recipeValid && recipe != null && recipe.MeltdownFlags.HasFlag(HypertorusFlags.EMP);

            if (recipeValid && recipe != null)
            {
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

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MinimumSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 3;
                        empHeavySize = core.PowerLevel * 1;
                    }
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MediumSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 5;
                        empHeavySize = core.PowerLevel * 3;
                    }
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.BigSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 7;
                        empHeavySize = core.PowerLevel * 5;
                    }
                }

                if (recipe.MeltdownFlags.HasFlag(HypertorusFlags.MassiveSpread))
                {
                    if (emPulse)
                    {
                        empLightSize = core.PowerLevel * 9;
                        empHeavySize = core.PowerLevel * 7;
                    }
                }
            }

            // Gas emission: Pour gases into the core's tile
            var transform = Transform(coreUid);
            var tileMixture = _atmosSystem.GetContainingMixture(coreUid, false, false);
            if (tileMixture != null)
            {
                if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0)
                {
                    _atmosSystem.Merge(tileMixture, core.InternalFusion);
                }

                if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0)
                {
                    _atmosSystem.Merge(tileMixture, core.ModeratorInternal);
                }
            }

            // Explosion
            var totalIntensity = (devastatingExplosion + heavyImpactExplosion + lightImpactExplosion + flashExplosion) * 10f;
            if (totalIntensity > 0)
            {
                _explosionSystem.QueueExplosion(
                    coreUid,
                    ExplosionSystem.DefaultExplosionPrototypeId,
                    totalIntensity,
                    slope: 10f,
                    maxTileIntensity: 100,
                    tileBreakScale: 1f,
                    maxTileBreak: int.MaxValue,
                    canCreateVacuum: true,
                    user: null,
                    addLog: true
                );
            }

            // EMP Pulse
            if (emPulse)
            {
                var empCoords = _transformSystem.GetMapCoordinates(coreUid);
                var empRange = critical ? empLightSize * 2 : empLightSize;
                var energyConsumption = empHeavySize * 1000f;
                var duration = 30f;
                _empSystem.EmpPulse(
                    empCoords,
                    range: empRange,
                    energyConsumption: energyConsumption,
                    duration: duration
                );
            }

            _entityManager.QueueDeleteEntity(coreUid);
        }

        /**
         * Formats time for countdown announcements
         */
        private string DisplayTimeText(int time, bool isSeconds)
        {
            if (isSeconds)
            {
                int seconds = time / 10;
                return $"{seconds} second{(seconds != 1 ? "s" : "")}";
            }
            return string.Empty;
        }
    }
}