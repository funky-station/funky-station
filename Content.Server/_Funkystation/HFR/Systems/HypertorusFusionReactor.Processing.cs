// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Funkystation.Atmos.Components;
using Content.Shared.Atmos;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Funkystation.Atmos.HFR;
using System.Linq;
using Robust.Shared.Random;
using Content.Shared._Funkystation.Atmos.Prototypes;
using Content.Server.Singularity.Components;
using Content.Shared.Radiation.Components;

namespace Content.Server._Funkystation.Atmos.HFR.Systems
{
    public sealed partial class HypertorusFusionReactorSystem
    {
        /**
         * Main Fusion processes
         * Process() Organizes all other calls, and is the best starting point for top-level logic.
         * FusionProcess() handles all the main fusion reaction logic and consequences (lightning, radiation, particles) from an active fusion reaction.
         */

        /**
         * Organizes all fusion process calls, serving as the top-level logic entry point.
         */
        public void Process(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            /*
             * Pre-checks
             */

            //core.AreaPower = GetAreaCellPercent(coreUid);
            HandleSoundLoop(coreUid, core);

            // Run the reaction if it is either live or being started
            if (core.IsActive || core.PowerLevel > 0)
            {
                PlayAccent(coreUid, core);
                FusionProcess(coreUid, core, secondsPerTick);
                // Note that we process damage/healing even if the fusion process aborts.
                // Running out of fuel won't save you if your moderator and coolant are exploding on their own.
                ProcessDamageHeal(coreUid, core, secondsPerTick);
                CheckAlert(coreUid, core);
            }

            if (core.IsActive)
                RemoveWaste(coreUid, core, secondsPerTick, core.IsWasteRemoving);

            // Check for gas spills due to cracked parts
            if (core.IsActive || core.PowerLevel > 0)
                CheckSpill(coreUid, core, secondsPerTick);
        }

        /**
         * Called by Process()
         * Contains the main fusion calculations and checks, for more information check the comments along the code.
         */
        public void FusionProcess(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            // Fusion: a terrible idea that was fun but broken. Now reworked to be less broken and more interesting. Again (and again, and again). Again! Again but with machine!
            // Fusion Rework Counter: Please increment this if you make a major overhaul to this system again.
            // 7 reworks

            // Check if the console exists and is powered
            bool isConsolePowered = core.ConsoleUid != null && _powerReceiver.IsPowered(core.ConsoleUid.Value);

            // Define local variables for settings
            float magneticConstrictor = isConsolePowered ? core.MagneticConstrictor : 100f;
            float heatingConductor = isConsolePowered ? core.HeatingConductor : 500f;
            float currentDamper = isConsolePowered ? core.CurrentDamper : 0f;
            float fuelInputRate = isConsolePowered ? core.FuelInputRate : 20f;
            float moderatorInputRate = isConsolePowered ? core.ModeratorInputRate : 50f;
            core.IsWasteRemoving = isConsolePowered ? core.IsWasteRemoving : false;

            if (isConsolePowered)
            {
                if (core.IsCooling)
                {
                    InjectFromSideComponents(coreUid, core, secondsPerTick, fuelInputRate, moderatorInputRate);
                    ProcessInternalCooling(coreUid, core, secondsPerTick);
                }
            }
            else
            {
                // Increase iron content if active and powered
                core.IronContent += 0.02f * core.PowerLevel * secondsPerTick;
            }

            UpdateTemperatureStatus(coreUid, core, secondsPerTick);

            // Store the temperature of the gases after one cycle of the fusion reaction
            float archivedHeat = core.InternalFusion?.Temperature ?? Atmospherics.T20C;
            // Store the volume of the fusion reaction multiplied by the force of the magnets that controls how big it will be
            float volume = (core.InternalFusion?.Volume ?? 5000f) * (magneticConstrictor * 0.01f);

            float energyConcentrationMultiplier = 1f;
            float positiveTemperatureMultiplier = 1f;
            float negativeTemperatureMultiplier = 1f;

            // We scale it down by volume/2 because for fusion conditions, moles roughly = 2*volume, but we want it to be based off something constant between reactions.
            float scaleFactor = volume * 0.5f;

            /// Store the fuel gases and the byproduct gas quantities
            var fuelList = new Dictionary<Gas, float>();
            /// Scaled down moles of gases, no less than 0
            var scaledFuelList = new Dictionary<Gas, float>();

            FusionRecipePrototype? recipe = null;
            float temperatureModifier = 1f;
            if (!string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe))
            {
                energyConcentrationMultiplier = recipe.EnergyConcentrationMultiplier;
                positiveTemperatureMultiplier = recipe.PositiveTemperatureMultiplier;
                negativeTemperatureMultiplier = recipe.NegativeTemperatureMultiplier;
                temperatureModifier = recipe.TemperatureChangeMultiplier;

                var requiredGases = recipe.Requirements.Concat(recipe.PrimaryProducts).Select(gasId => Enum.Parse<Gas>(gasId)).Distinct();
                foreach (var gas in requiredGases)
                {
                    float amount = core.InternalFusion?.GetMoles(gas) ?? 0f;
                    fuelList[gas] = amount;
                    scaledFuelList[gas] = Math.Max((amount - HypertorusFusionReactor.FusionMoleThreshold) / scaleFactor, 0f);
                }
            }

            /// Store the moderators gases quantities
            var moderatorList = new Dictionary<Gas, float>();
            /// Scaled down moles of gases, no less than 0
            var scaledModeratorList = new Dictionary<Gas, float>();
            if (core.ModeratorInternal != null)
            {
                foreach (var gas in Enum.GetValues<Gas>())
                {
                    float amount = core.ModeratorInternal.GetMoles(gas);
                    moderatorList[gas] = amount;
                    scaledModeratorList[gas] = Math.Max((amount - HypertorusFusionReactor.FusionMoleThreshold) / scaleFactor, 0f);
                }
            }

            /*
             * FUSION MAIN PROCESS
             */
            // This section is used for the instability calculation for the fusion reaction
            // The size of the phase space hypertorus
            float toroidalSize = (2 * MathF.PI) + MathF.Atan((volume - HypertorusFusionReactor.ToroidVolumeBreakeven) / HypertorusFusionReactor.ToroidVolumeBreakeven);
            // Calculation of the gas power, only for theoretical instability calculations
            float gasPower = 0f;
            if (core.InternalFusion != null)
            {
                foreach (var gas in Enum.GetValues<Gas>())
                {
                    gasPower += (HypertorusFusionReactor.GasFusionPower(gas) * core.InternalFusion.GetMoles(gas));
                }
            }
            if (core.ModeratorInternal != null)
            {
                foreach (var gas in Enum.GetValues<Gas>())
                {
                    gasPower += (HypertorusFusionReactor.GasFusionPower(gas) * core.ModeratorInternal.GetMoles(gas) * 0.75f);
                }
            }


            // Calculate instability
            float gasPowerFactor = MathF.Pow(gasPower * HypertorusFusionReactor.InstabilityGasPowerFactor, 2);
            float gasPowerRemainder = gasPowerFactor % toroidalSize;
            float damper = currentDamper * 0.01f;
            float ironContent = core.IronContent * 0.05f;
            core.Instability = gasPowerRemainder + damper - ironContent;

            // Effective reaction instability (determines if the energy is used/released)
            float internalInstability = core.Instability * 0.5f < HypertorusFusionReactor.FusionInstabilityEndothermality ? 1f : -1f;

            /*
            * Modifiers
            */
            /// Those are the scaled gases that get consumed and adjust energy
            // Gases that increase the amount of energy
            float energyModifiers = scaledModeratorList.GetValueOrDefault(Gas.Nitrogen, 0f) * 0.35f +
                                scaledModeratorList.GetValueOrDefault(Gas.CarbonDioxide, 0f) * 0.55f +
                                scaledModeratorList.GetValueOrDefault(Gas.NitrousOxide, 0f) * 0.95f +
                                scaledModeratorList.GetValueOrDefault(Gas.Zauker, 0f) * 1.55f +
                                scaledModeratorList.GetValueOrDefault(Gas.AntiNoblium, 0f) * 20f;
            // Gases that decrease the amount of energy
            energyModifiers -= scaledModeratorList.GetValueOrDefault(Gas.HyperNoblium, 0f) * 10f +
                            scaledModeratorList.GetValueOrDefault(Gas.WaterVapor, 0f) * 0.75f +
                            scaledModeratorList.GetValueOrDefault(Gas.Nitrium, 0f) * 0.15f +
                            scaledModeratorList.GetValueOrDefault(Gas.Healium, 0f) * 0.45f +
                            scaledModeratorList.GetValueOrDefault(Gas.Frezon, 0f) * 1.15f;
            /// Between 0.25 and 100, this value is used to modify the behaviour of the internal energy and the core temperature based on the gases present in the mix
            float powerModifier = scaledModeratorList.GetValueOrDefault(Gas.Oxygen, 0f) * 0.55f +
                                scaledModeratorList.GetValueOrDefault(Gas.CarbonDioxide, 0f) * 0.95f +
                                scaledModeratorList.GetValueOrDefault(Gas.Nitrium, 0f) * 1.45f +
                                scaledModeratorList.GetValueOrDefault(Gas.Zauker, 0f) * 5.55f +
                                scaledModeratorList.GetValueOrDefault(Gas.Plasma, 0f) * 0.05f -
                                scaledModeratorList.GetValueOrDefault(Gas.NitrousOxide, 0f) * 0.05f -
                                scaledModeratorList.GetValueOrDefault(Gas.Frezon, 0f) * 0.75f;
            /// Minimum 0.25, this value is used to modify the behaviour of the energy emission based on the gases present in the mix
            float heatModifier = scaledModeratorList.GetValueOrDefault(Gas.Plasma, 0f) * 1.25f -
                                scaledModeratorList.GetValueOrDefault(Gas.Nitrogen, 0f) * 0.75f -
                                scaledModeratorList.GetValueOrDefault(Gas.NitrousOxide, 0f) * 1.45f -
                                scaledModeratorList.GetValueOrDefault(Gas.Frezon, 0f) * 0.95f;
            /// Between 0.005 and 1000, this value modifies the radiation emission of the reaction, higher values increase the emission
            float radiationModifier = scaledModeratorList.GetValueOrDefault(Gas.Frezon, 0f) * 1.15f -
                                    scaledModeratorList.GetValueOrDefault(Gas.Nitrogen, 0f) * 0.45f -
                                    scaledModeratorList.GetValueOrDefault(Gas.Plasma, 0f) * 0.95f +
                                    scaledModeratorList.GetValueOrDefault(Gas.BZ, 0f) * 1.9f +
                                    scaledModeratorList.GetValueOrDefault(Gas.ProtoNitrate, 0f) * 0.1f +
                                    scaledModeratorList.GetValueOrDefault(Gas.AntiNoblium, 0f) * 10f;

            if (recipe != null)
            {
                var fuel1 = recipe.Requirements.Count > 0 ? Enum.Parse<Gas>(recipe.Requirements[0]) : Gas.Hydrogen;
                var fuel2 = recipe.Requirements.Count > 1 ? Enum.Parse<Gas>(recipe.Requirements[1]) : Gas.Tritium;
                var product = recipe.PrimaryProducts.Count > 0 ? Enum.Parse<Gas>(recipe.PrimaryProducts[0]) : Gas.Helium;
                energyModifiers += scaledFuelList.GetValueOrDefault(fuel1, 0f) +
                                scaledFuelList.GetValueOrDefault(fuel2, 0f) -
                                scaledFuelList.GetValueOrDefault(product, 0f);

                powerModifier += scaledFuelList.GetValueOrDefault(fuel2, 0f) * 1.05f -
                                scaledFuelList.GetValueOrDefault(product, 0f) * 0.55f;

                heatModifier += scaledFuelList.GetValueOrDefault(fuel1, 0f) * 1.15f +
                                scaledFuelList.GetValueOrDefault(product, 0f) * 1.05f;

                radiationModifier += scaledFuelList.GetValueOrDefault(product, 0f);
            }

            powerModifier = Math.Clamp(powerModifier, 0.25f, 100f);
            heatModifier = Math.Clamp(heatModifier, 0.25f, 100f);
            radiationModifier = Math.Clamp(radiationModifier, 0.005f, 1000f);

            /*
             * Main calculations (energy, internal power, core temperature, delta temperature,
             * conduction, radiation, efficiency, power output, heat limiter modifier and heat output)
             */
            core.InternalPower = 0f;
            core.Efficiency = HypertorusFusionReactor.VoidConduction * 1f;

            if (recipe != null)
            {
                var fuel1 = recipe.Requirements.Count > 0 ? Enum.Parse<Gas>(recipe.Requirements[0]) : Gas.Hydrogen;
                var fuel2 = recipe.Requirements.Count > 1 ? Enum.Parse<Gas>(recipe.Requirements[1]) : Gas.Tritium;
                var product = recipe.PrimaryProducts.Count > 0 ? Enum.Parse<Gas>(recipe.PrimaryProducts[0]) : Gas.Helium;
                // Power of the gas mixture
                core.InternalPower = (scaledFuelList.GetValueOrDefault(fuel1, 0f) * powerModifier / 100f) *
                                     (scaledFuelList.GetValueOrDefault(fuel2, 0f) * powerModifier / 100f) *
                                     (MathF.PI * MathF.Pow(2 * (scaledFuelList.GetValueOrDefault(fuel1, 0f) * HypertorusFusionReactor.CalculatedH2Radius) *
                                     (scaledFuelList.GetValueOrDefault(fuel2, 0f) * HypertorusFusionReactor.CalculatedTritRadius), 2)) * core.Energy;

                // Efficiency of the reaction, it increases with the amount of byproduct
                float tickRateAdjustment = secondsPerTick / 2f; // Normalize to SS13's 2-second tick rate for proper efficiency calculation
                float adjustedScaledFuel = scaledFuelList.GetValueOrDefault(product, 0f) / tickRateAdjustment;
                core.Efficiency = HypertorusFusionReactor.VoidConduction * Math.Clamp(adjustedScaledFuel, 1f, 100f);
            }

            // Can go either positive or negative depending on the instability and the negative energy modifiers
            // E=mc^2 with some changes for gameplay purposes
            core.Energy = (energyModifiers * MathF.Pow(HypertorusFusionReactor.LightSpeed, 2)) * Math.Max((core.InternalFusion?.Temperature ?? Atmospherics.T20C) * heatModifier / 100f, 1f);
            core.Energy /= energyConcentrationMultiplier;
            core.Energy = Math.Clamp(core.Energy, 0f, 1e35f); // Ugly way to prevent NaN error
            // Temperature inside the center of the gas mixture
            core.CoreTemperature = core.InternalPower * powerModifier / 1000f;
            core.CoreTemperature = Math.Max(Atmospherics.TCMB, core.CoreTemperature);
            // Difference between the gases temperature and the internal temperature of the reaction
            core.DeltaTemperature = archivedHeat - core.CoreTemperature;
            // Energy from the reaction lost from the molecule colliding between themselves.
            core.Conduction = -core.DeltaTemperature * (magneticConstrictor * 0.001f);
            // The remaining wavelength that actually can do damage to mobs.
            core.Radiation = Math.Max(-(HypertorusFusionReactor.PlanckLightConstant / 5e-18f) * radiationModifier * core.DeltaTemperature, 0f);
            core.PowerOutput = core.Efficiency * (core.InternalPower - core.Conduction - core.Radiation);
            // Hotter air is easier to heat up and cool down
            core.HeatLimiterModifier = 10f * MathF.Pow(10, core.PowerLevel) * (heatingConductor / 100f);
            // The amount of heat that is finally emitted, based on the power output. Min and max are variables that depend on the modifier
            core.HeatOutputMin = -core.HeatLimiterModifier * 0.01f * negativeTemperatureMultiplier;
            core.HeatOutputMax = core.HeatLimiterModifier * positiveTemperatureMultiplier;
            core.HeatOutput = Math.Clamp(internalInstability * core.PowerOutput * heatModifier / 100f, core.HeatOutputMin, core.HeatOutputMax);

            // Is the fusion process actually going to run?
            // Note we have to always perform the above calculations to keep the UI updated, so we can't use this to early return.
            if (!CheckFuel(core))
                return;

            // Phew. Let's calculate what this means in practice.
            float fuelConsumptionRate = Math.Clamp(fuelInputRate * 0.01f * 5f * core.PowerLevel, 0.05f, 30f);
            float consumptionAmount = fuelConsumptionRate * secondsPerTick;
            float productionAmount = core.PowerLevel switch
            {
                3 or 4 => Math.Clamp(core.HeatOutput * 5e-4f, 0f, fuelConsumptionRate) * secondsPerTick,
                _ => Math.Clamp(core.HeatOutput / MathF.Pow(10, core.PowerLevel + 1), 0f, fuelConsumptionRate) * secondsPerTick
            };

            // Antinob production is special, and uses its own calculations from how stale the fusion mix is (via byproduct ratio and fresh fuel rate)
            float dirtyProductionRate = scaledFuelList.GetValueOrDefault(Gas.Helium, 0f) / fuelInputRate;

            // Run the effects of our selected fuel recipe
            var internalOutput = new GasMixture();
            ModeratorFuelProcess(core, secondsPerTick, productionAmount, consumptionAmount, internalOutput, moderatorList, recipe, fuelList);

            // Run the common effects, committing changes where applicable
            // This is repetition, but is here as a placeholder for what will need to be done to allow concurrently running multiple recipes
            float commonProductionAmount = productionAmount * (recipe?.GasProductionMultiplier ?? 1f);
            ModeratorCommonProcess(coreUid, core, secondsPerTick, commonProductionAmount, internalOutput, moderatorList, dirtyProductionRate, core.HeatOutput, radiationModifier, temperatureModifier);
        }

        /**
        * Perform recipe-specific actions. Fuel consumption and recipe-based gas production happens here.
        */
        public void ModeratorFuelProcess(HFRCoreComponent core, float secondsPerTick, float productionAmount, float consumptionAmount, GasMixture internalOutput, Dictionary<Gas, float> moderatorList, FusionRecipePrototype? fuel, Dictionary<Gas, float> fuelList)
        {
            if (fuel == null || core.InternalFusion == null)
                return;

            // Adjust fusion consumption/production based on this recipe's characteristics
            float fuelConsumption = consumptionAmount * 0.85f * fuel.FuelConsumptionMultiplier;
            float scaledProduction = productionAmount * fuel.GasProductionMultiplier;

            foreach (var gas in fuel.Requirements.Select(gasId => Enum.Parse<Gas>(gasId)))
            {
                core.InternalFusion.SetMoles(gas, Math.Max(core.InternalFusion.GetMoles(gas) - Math.Min(fuelList.GetValueOrDefault(gas, 0f), fuelConsumption), 0f));
            }

            foreach (var gas in fuel.PrimaryProducts.Select(gasId => Enum.Parse<Gas>(gasId)))
            {
                core.InternalFusion.AdjustMoles(gas, fuelConsumption * 0.5f);
            }

            // Each recipe provides a tier list of output gases.
            // Which gases are produced depend on what the fusion level is.
            var tier = fuel.SecondaryProducts;
            switch (core.PowerLevel)
            {
                case 1:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[0]), scaledProduction * 0.95f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[1]), scaledProduction * 0.75f);
                    break;
                case 2:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[0]), scaledProduction * 1.65f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[1]), scaledProduction);
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 50f)
                        core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[2]), scaledProduction * 1.15f);

                    break;
                case 3:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[1]), scaledProduction * 0.5f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[2]), scaledProduction * 0.45f);
                    break;
                case 4:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[2]), scaledProduction * 1.65f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[3]), scaledProduction * 1.25f);
                    break;
                case 5:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[3]), scaledProduction * 0.65f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[4]), scaledProduction);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[5]), scaledProduction * 0.75f);
                    break;
                case 6:
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[4]), scaledProduction * 0.35f);
                    core.ModeratorInternal?.AdjustMoles(Enum.Parse<Gas>(tier[5]), scaledProduction);
                    break;
            }
        }

        /**
        * Perform common fusion actions:
        *
        * - Gases that get produced irrespective of recipe
        * - Temperature modifiers, radiation modifiers, and the application of each
        * - Committing staged output, performing filtering, and making !FUN! emissions
        */
        public void ModeratorCommonProcess(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick, float scaledProduction, GasMixture internalOutput, Dictionary<Gas, float> moderatorList, float dirtyProductionRate, float heatOutput, float radiationModifier, float temperatureModifier)
        {
            float modifiedHeatOutput = heatOutput;
            float modifiedRadiation = core.Radiation;

            switch (core.PowerLevel)
            {
                case 1:
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 100f)
                    {
                        internalOutput.AdjustMoles(Gas.NitrousOxide, scaledProduction * 0.5f);
                        core.ModeratorInternal?.AdjustMoles(Gas.Plasma, -Math.Min(moderatorList.GetValueOrDefault(Gas.Plasma, 0f), scaledProduction * 0.85f));
                    }
                    if (moderatorList.GetValueOrDefault(Gas.BZ, 0f) > 150f)
                    {
                        internalOutput.AdjustMoles(Gas.Halon, scaledProduction * 0.55f);
                        core.ModeratorInternal?.AdjustMoles(Gas.BZ, -Math.Min(moderatorList.GetValueOrDefault(Gas.BZ, 0f), scaledProduction * 0.95f));
                    }
                    break;
                case 2:
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 50f)
                    {
                        internalOutput.AdjustMoles(Gas.BZ, scaledProduction * 1.8f);
                        core.ModeratorInternal?.AdjustMoles(Gas.Plasma, -Math.Min(moderatorList.GetValueOrDefault(Gas.Plasma, 0f), scaledProduction * 1.75f));
                    }
                    if (moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f) > 20f)
                    {
                        modifiedRadiation *= 1.55f;
                        modifiedHeatOutput *= 1.025f;
                        internalOutput.AdjustMoles(Gas.Nitrium, scaledProduction * 1.05f);
                        core.ModeratorInternal?.AdjustMoles(Gas.ProtoNitrate, -Math.Min(moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f), scaledProduction * 1.35f));
                    }
                    break;
                case 3 or 4:
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 10f)
                    {
                        internalOutput.AdjustMoles(Gas.Frezon, scaledProduction * 0.15f);
                        internalOutput.AdjustMoles(Gas.Nitrium, scaledProduction * 1.05f);
                        core.ModeratorInternal?.AdjustMoles(Gas.Plasma, -Math.Min(moderatorList.GetValueOrDefault(Gas.Plasma, 0f), scaledProduction * 0.45f));
                    }
                    if (moderatorList.GetValueOrDefault(Gas.Frezon, 0f) > 50f)
                    {
                        modifiedHeatOutput *= 0.9f;
                        modifiedRadiation *= 0.8f;
                    }
                    if (moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f) > 15f)
                    {
                        internalOutput.AdjustMoles(Gas.Nitrium, scaledProduction * 1.25f);
                        internalOutput.AdjustMoles(Gas.Halon, scaledProduction * 1.15f);
                        core.ModeratorInternal?.AdjustMoles(Gas.ProtoNitrate, -Math.Min(moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f), scaledProduction * 1.55f));
                        modifiedRadiation *= 1.95f;
                        modifiedHeatOutput *= 1.25f;
                    }
                    if (moderatorList.GetValueOrDefault(Gas.BZ, 0f) > 100f)
                    {
                        internalOutput.AdjustMoles(Gas.ProtoNitrate, scaledProduction * 1.5f);
                        internalOutput.AdjustMoles(Gas.Healium, scaledProduction * 1.5f);
                    }
                    break;
                case 5:
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 15f)
                    {
                        internalOutput.AdjustMoles(Gas.Frezon, scaledProduction * 0.25f);
                        core.ModeratorInternal?.AdjustMoles(Gas.Plasma, -Math.Min(moderatorList.GetValueOrDefault(Gas.Plasma, 0f), scaledProduction * 1.45f));
                    }
                    if (moderatorList.GetValueOrDefault(Gas.Frezon, 0f) > 500f)
                    {
                        modifiedHeatOutput *= 0.5f;
                        modifiedRadiation *= 0.2f;
                    }
                    if (moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f) > 50f)
                    {
                        internalOutput.AdjustMoles(Gas.Nitrium, scaledProduction * 1.95f);
                        internalOutput.AdjustMoles(Gas.Pluoxium, scaledProduction);
                        core.ModeratorInternal?.AdjustMoles(Gas.ProtoNitrate, -Math.Min(moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f), scaledProduction * 1.35f));
                        modifiedRadiation *= 1.95f;
                        modifiedHeatOutput *= 1.25f;
                    }
                    if (moderatorList.GetValueOrDefault(Gas.BZ, 0f) > 100f)
                    {
                        internalOutput.AdjustMoles(Gas.Healium, scaledProduction);
                        internalOutput.AdjustMoles(Gas.Frezon, scaledProduction * 1.15f);
                    }
                    if (moderatorList.GetValueOrDefault(Gas.Healium, 0f) > 100f)
                    {
                        if (core.CriticalThresholdProximity > 400f)
                        {
                            float healiumAmount = moderatorList.GetValueOrDefault(Gas.Healium, 0f);
                            float healApplied = -(healiumAmount / 100f * secondsPerTick);
                            float previousProximity = core.CriticalThresholdProximity;
                            core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + healApplied, 0f);
                            core.ModeratorInternal?.AdjustMoles(Gas.Healium, -Math.Min(moderatorList.GetValueOrDefault(Gas.Healium, 0f), scaledProduction * 20f));
                        }
                    }
                    if ((core.ModeratorInternal?.Temperature < 1e7f) || (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 100f && moderatorList.GetValueOrDefault(Gas.BZ, 0f) > 50f))
                    {
                        internalOutput.AdjustMoles(Gas.AntiNoblium, dirtyProductionRate * 0.9f / 0.065f * secondsPerTick);
                    }
                    break;
                case 6:
                    if (moderatorList.GetValueOrDefault(Gas.Plasma, 0f) > 30f)
                    {
                        internalOutput.AdjustMoles(Gas.BZ, scaledProduction * 1.15f);
                        core.ModeratorInternal?.AdjustMoles(Gas.Plasma, -Math.Min(moderatorList.GetValueOrDefault(Gas.Plasma, 0f), scaledProduction * 1.45f));
                    }
                    if (moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f) > 0f)
                    {
                        internalOutput.AdjustMoles(Gas.Zauker, scaledProduction * 5.35f);
                        internalOutput.AdjustMoles(Gas.Nitrium, scaledProduction * 2.15f);
                        core.ModeratorInternal?.AdjustMoles(Gas.ProtoNitrate, -Math.Min(moderatorList.GetValueOrDefault(Gas.ProtoNitrate, 0f), scaledProduction * 3.35f));
                        modifiedRadiation *= 2f;
                        modifiedHeatOutput *= 2.25f;
                    }
                    if (moderatorList.GetValueOrDefault(Gas.BZ, 0f) > 0f)
                    {
                        internalOutput.AdjustMoles(Gas.AntiNoblium, Math.Clamp(dirtyProductionRate / 0.045f, 0f, 10f) * secondsPerTick);
                    }
                    if (moderatorList.GetValueOrDefault(Gas.Healium, 0f) > 100f)
                    {
                        if (core.CriticalThresholdProximity > 400f)
                        {
                            float healiumAmount = moderatorList.GetValueOrDefault(Gas.Healium, 0f);
                            float healApplied = -(healiumAmount / 100f * secondsPerTick);
                            float previousProximity = core.CriticalThresholdProximity;
                            core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + healApplied, 0f);
                            core.ModeratorInternal?.AdjustMoles(Gas.Healium, -Math.Min(moderatorList.GetValueOrDefault(Gas.Healium, 0f), scaledProduction * 20f));
                        }
                    }
                    core.InternalFusion?.AdjustMoles(Gas.AntiNoblium, dirtyProductionRate * 0.01f / 0.095f * secondsPerTick);
                    break;
            }

            // Modifies the internal_fusion temperature with the amount of heat output
            if (core.InternalFusion != null)
            {
                if (core.InternalFusion.Temperature <= HypertorusFusionReactor.FusionMaximumTemperature * temperatureModifier)
                {
                    core.InternalFusion.Temperature = Math.Clamp(core.InternalFusion.Temperature + core.HeatOutput, Atmospherics.TCMB, HypertorusFusionReactor.FusionMaximumTemperature * temperatureModifier);
                }
                else
                {
                    core.InternalFusion.Temperature -= core.HeatLimiterModifier * 0.01f * secondsPerTick;
                }
            }

            // Heat up and output what's in the internal_output into the linked_output port
            if (internalOutput.TotalMoles > 0f && core.WasteOutputUid != null && _nodeContainer.TryGetNode(core.WasteOutputUid.Value, "pipe", out PipeNode? wastePipe))
            {
                internalOutput.Temperature = core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0f
                    ? core.ModeratorInternal.Temperature * HypertorusFusionReactor.HighEfficiencyConductivity
                    : core.InternalFusion?.Temperature * HypertorusFusionReactor.MetallicVoidConductivity ?? Atmospherics.T20C;
                _atmosSystem.Merge(wastePipe.Air, internalOutput);
            }

            EvaporateModerator(core, secondsPerTick);

            ProcessRadiation(coreUid, core, radiationModifier, secondsPerTick);

            CheckLightningArcs(coreUid, core, moderatorList);

            // Oxygen burns away iron content rapidly
            if (moderatorList.GetValueOrDefault(Gas.Oxygen, 0f) > 150f && core.IronContent > 0f)
            {
                float maxIronRemovable = HypertorusFusionReactor.IronOxygenHealPerSecond;
                float ironRemoved = Math.Min(maxIronRemovable * secondsPerTick, core.IronContent);
                core.IronContent -= ironRemoved;
                core.ModeratorInternal?.AdjustMoles(Gas.Oxygen, -ironRemoved * HypertorusFusionReactor.OxygenMolesConsumedPerIronHeal);
            }

            CheckGravityPulse(coreUid, core, secondsPerTick);
        }

        public void EvaporateModerator(HFRCoreComponent core, float secondsPerTick)
        {
            // Don't evaporate if the reaction is dead
            if (core.PowerLevel == 0)
                return;

            // All gases in the moderator slowly burn away over time, whether used for production or not
            if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0f)
            {
                core.ModeratorInternal.Remove(core.ModeratorInternal.TotalMoles * (1f - MathF.Pow(1f - 0.0005f * core.PowerLevel, secondsPerTick)));
            }
        }

        public void ProcessDamageHeal(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            // Archive current health for damage cap purposes
            core.CriticalThresholdProximityArchived = core.CriticalThresholdProximity;

            // Reset damage check flags
            core.StatusFlags &= HypertorusStatusFlags.Emped;

            // If we're operating at an extreme power level, take increasing damage for the amount of fusion mass over a low threshold
            if (core.PowerLevel >= HypertorusFusionReactor.HypertorusOverfullMinPowerLevel)
            {
                float overfullDamageTaken = HypertorusFusionReactor.HypertorusOverfullMolarSlope * (core.InternalFusion?.TotalMoles ?? 0f) +
                                            HypertorusFusionReactor.HypertorusOverfullTemperatureSlope * core.CoolantTemperature +
                                            HypertorusFusionReactor.HypertorusOverfullConstant;
                float damageApplied = Math.Max(overfullDamageTaken * secondsPerTick, 0f);
                float previousProximity = core.CriticalThresholdProximity;
                core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + damageApplied, 0f);
                core.StatusFlags |= HypertorusStatusFlags.HighPowerDamage;
            }

            // If we're running on a thin fusion mix, heal up
            if ((core.InternalFusion?.TotalMoles ?? 0f) < HypertorusFusionReactor.HypertorusSubcriticalMoles && core.PowerLevel <= 5)
            {
                float subcriticalHealRestore = ((core.InternalFusion?.TotalMoles ?? 0f) - HypertorusFusionReactor.HypertorusSubcriticalMoles) / HypertorusFusionReactor.HypertorusSubcriticalScale;
                float healApplied = Math.Min(subcriticalHealRestore * secondsPerTick, 0f);
                float previousProximity = core.CriticalThresholdProximity;
                core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + healApplied, 0f);
            }

            // If coolant is sufficiently cold, heal up
            if ((core.InternalFusion?.TotalMoles ?? 0f) > 0f && _nodeContainer.TryGetNode(coreUid, "pipe", out PipeNode? corePipe) &&
                corePipe.Air.TotalMoles > 0f && core.CoolantTemperature < HypertorusFusionReactor.HypertorusColdCoolantThreshold && core.PowerLevel <= 4)
            {
                float coldCoolantHealRestore = MathF.Log10(Math.Max(core.CoolantTemperature, 1f) * HypertorusFusionReactor.HypertorusColdCoolantScale) - (HypertorusFusionReactor.HypertorusColdCoolantMaxRestore * 2f);
                float healApplied = Math.Min(coldCoolantHealRestore * secondsPerTick, 0f);
                float previousProximity = core.CriticalThresholdProximity;
                core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + healApplied, 0f);
            }

            // Iron content damage
            float ironDamage = Math.Max(core.IronContent - HypertorusFusionReactor.HypertorusMaxSafeIron, 0f) * secondsPerTick;
            float previousIronProximity = core.CriticalThresholdProximity;
            core.CriticalThresholdProximity += ironDamage;
            if (core.IronContent - HypertorusFusionReactor.HypertorusMaxSafeIron > 0f)
                core.StatusFlags |= HypertorusStatusFlags.IronContentDamage;

            // Apply damage cap
            float damageCap = core.CriticalThresholdProximityArchived + (secondsPerTick * HypertorusFusionReactor.DamageCapMultiplier * core.MeltingPoint);
            float previousCapProximity = core.CriticalThresholdProximity;
            core.CriticalThresholdProximity = Math.Min(damageCap, core.CriticalThresholdProximity);

            // If we have a preposterous amount of mass in the fusion mix, things get bad extremely fast
            if ((core.InternalFusion?.TotalMoles ?? 0f) >= HypertorusFusionReactor.HypertorusHypercriticalMoles)
            {
                float hypercriticalDamageTaken = Math.Max(((core.InternalFusion?.TotalMoles ?? 0f) - HypertorusFusionReactor.HypertorusHypercriticalMoles) * HypertorusFusionReactor.HypertorusHypercriticalScale, 0f);
                float hypercriticalDamageApplied = Math.Min(hypercriticalDamageTaken, HypertorusFusionReactor.HypertorusHypercriticalMaxDamage) * secondsPerTick;
                float previousProximity = core.CriticalThresholdProximity;
                core.CriticalThresholdProximity = Math.Max(core.CriticalThresholdProximity + hypercriticalDamageApplied, 0f);
                core.StatusFlags |= HypertorusStatusFlags.HighFuelMixMole;
            }

            // High power fusion might create other matter other than helium, iron is dangerous inside the machine, damage can be seen
            if (core.PowerLevel > 4 && _random.Prob(Math.Clamp(HypertorusFusionReactor.IronChancePerFusionLevel * core.PowerLevel, 0f, 100f) / 100f))
            {
                float previousIronContent = core.IronContent;
                core.IronContent += HypertorusFusionReactor.IronAccumulatedPerSecond * secondsPerTick;
                core.StatusFlags |= HypertorusStatusFlags.IronContentIncrease;
            }
            if (core.IronContent > 0f && core.PowerLevel <= 4 && _random.Prob(Math.Clamp(25f / (core.PowerLevel + 1), 0f, 100f) / 100f))
            {
                float previousIronContent = core.IronContent;
                core.IronContent = Math.Max(core.IronContent - 0.01f * secondsPerTick, 0f);
            }
            core.IronContent = Math.Clamp(core.IronContent, 0f, 1f);
        }

        public void ProcessRadiation(EntityUid coreUid, HFRCoreComponent core, float radiationModifier, float secondsPerTick) // This is all bad someone please fix this
        {
            if (!_entityManager.TryGetComponent<RadiationSourceComponent>(coreUid, out var radSource))
                return;

            if (core.PowerLevel < 2)
            {
                radSource.Intensity = 0.001f;
                radSource.Slope = 0.2f;
                return;
            }

            if (radiationModifier < 0.1f && core.CriticalThresholdProximity < 200f)
            {
                radSource.Intensity = 0.001f;
                radSource.Slope = 0.2f;
                return;
            }

            // Base intensity scales with PowerLevel
            float baseIntensity = core.PowerLevel switch
            {
                2 => 3f,  // Low radiation at minimal active level
                3 => 6f,
                4 => 10f,
                5 => 15f,
                6 => 20f, // High radiation at max power
                _ => 0f
            };

            // Amplify intensity based on radiationModifier (0.005 to 1000)
            float intensity = baseIntensity * MathF.Sqrt(radiationModifier);

            // Boost intensity if critical threshold is high
            if (core.CriticalThresholdProximity > 650f)
                intensity *= 1.5f;

            // Bring intensity back down to something more reasonable
            intensity = intensity * 0.4f;

            // Calculate slope based on intensity I guess, directly stolen from supermatter
            float slope = Math.Clamp(intensity / 15f, 0.2f, 1f);

            // Apply radiation settings to the component
            radSource.Intensity = intensity;
            radSource.Slope = slope;
        }

        public void CheckLightningArcs(EntityUid coreUid, HFRCoreComponent core, Dictionary<Gas, float> moderatorList)
        {
            // Only trigger lightning arcs at power level 4 or higher
            if (core.PowerLevel < 4)
                return;

            // Require significant AntiNoblium or high critical threshold proximity
            if (moderatorList.GetValueOrDefault(Gas.AntiNoblium, 0f) <= 50f && core.CriticalThresholdProximity <= 500f)
                return;

            // Check if it's too early for the next zap
            if (_timing.CurTime < core.NextZap)
                return;

            // Base zap count starts at power level minus 2
            int zapNumber = core.PowerLevel - 2;
            // Increase zap count if critical threshold is high and random chance is met
            if (core.CriticalThresholdProximity > 650f && _random.Prob(0.2f))
                zapNumber++;

            // Calculate zap range based on power level and fusion mass
            float zapRange = Math.Clamp(core.PowerLevel * 2.4f, 5f, 10f);

            // Determine zap power
            int zapPower = core.PowerLevel switch
            {
                5 => 1, // SupermatterLightningCharged
                6 => 2, // SupermatterLightningSupercharged
                _ => 0  // SupermatterLightning
            };

            // Trigger lightning zaps
            HFRZap(coreUid, zapRange, zapNumber, zapPower);

            // Set the next zap time: current time + random interval based on power level
            float nextInterval = core.PowerLevel switch
            {
                4 => _random.NextFloat(12f, 20f),
                5 => _random.NextFloat(8f, 16f),
                6 => _random.NextFloat(1.5f, 6f),
                _ => _random.NextFloat(1.5f, 6f)
            };
            core.NextZap = _timing.CurTime + TimeSpan.FromSeconds(nextInterval);
        }

        public void CheckGravityPulse(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            if (!TryComp<GravityWellComponent>(coreUid, out var gravityWell))
                return;

            // Only trigger gravity pulses at PowerLevel 3 or higher
            if (core.PowerLevel < 3)
                return;

            // Update the next pulse time if not set
            if (core.NextGravityPulse == TimeSpan.Zero)
            {
                core.NextGravityPulse = _timing.CurTime + TimeSpan.FromSeconds(GetPulsePeriod(core));
            }

            // Check if it's time to pulse
            if (_timing.CurTime >= core.NextGravityPulse)
            {
                // Calculate pulse range based on PowerLevel
                float maxRange = core.PowerLevel switch
                {
                    3 => 1.0f, // Weak pull at low power
                    4 => 2.0f,
                    5 => 3.0f,
                    6 => 4.0f, // Strong pull at max power
                    _ => 0.5f
                };

                // Amplify range slightly if critical threshold is high
                if (core.CriticalThresholdProximity > 650f)
                    maxRange *= 1.2f;

                // Clamp range to reasonable bounds
                maxRange = Math.Clamp(maxRange, 0.5f, 3.0f);

                // Update gravity well's MaxRange
                gravityWell.MaxRange = maxRange;

                // Trigger the gravitational pulse
                _gravityWell.GravPulse(coreUid, maxRange, 0.0f, gravityWell.BaseRadialAcceleration, gravityWell.BaseTangentialAcceleration);

                // Set the next pulse time
                core.NextGravityPulse = _timing.CurTime + TimeSpan.FromSeconds(GetPulsePeriod(core));
            }
        }

        private float GetPulsePeriod(HFRCoreComponent core)
        {
            // Base period: shorter at higher PowerLevel
            float basePeriod = core.PowerLevel switch
            {
                3 => 10.0f,
                4 => 7.0f,
                5 => 5.0f,
                6 => 3.0f,
                _ => 10.0f
            };

            // Reduce period (more frequent pulses) if critical threshold is high
            if (core.CriticalThresholdProximity > 650f)
                basePeriod *= 0.8f;

            // Add randomization
            float randomFactor = _random.NextFloat(0.8f, 1.2f);
            return Math.Clamp(basePeriod * randomFactor, 2.0f, 12.0f);
        }

        public void RemoveWaste(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick, bool isWasteRemoving)
        {
            // Gases can be removed from the moderator internal by using the interface.
            if (!isWasteRemoving
                || core.WasteOutputUid == null
                || !_nodeContainer.TryGetNode(core.WasteOutputUid.Value, "pipe", out PipeNode? wastePipe)
                || wastePipe.Air.TotalMoles > 5000f)
                return;

            var moderatorRemovedMix = new GasMixture();
            var fusionRemovedMix = new GasMixture();

            int filteringAmount = 0;
            if (core.ModeratorInternal != null)
            {
                foreach (var gas in core.FilterGases)
                {
                    if (core.ModeratorInternal.GetMoles(gas) > 0.01f)
                        filteringAmount++;
                }
            }

            if (filteringAmount > 0 && core.ModeratorInternal != null)
            {
                foreach (var gas in core.FilterGases)
                {
                    float moles = core.ModeratorInternal.GetMoles(gas);
                    if (moles > 0f)
                    {
                        float removeAmount = (core.ModeratorFilteringRate / filteringAmount) * secondsPerTick;
                        removeAmount = Math.Min(removeAmount, moles);
                        moderatorRemovedMix.AdjustMoles(gas, removeAmount);
                        core.ModeratorInternal.SetMoles(gas, moles - removeAmount);
                    }
                }

                moderatorRemovedMix.Temperature = core.ModeratorInternal.Temperature;
            }

            FusionRecipePrototype? recipe = null;
            if (!string.IsNullOrEmpty(core.SelectedRecipeId) && _prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe) && core.InternalFusion != null)
            {
                foreach (var gas in recipe.PrimaryProducts.Select(gasId => Enum.Parse<Gas>(gasId)))
                {
                    float moles = core.InternalFusion.GetMoles(gas);
                    if (moles > 0f)
                    {
                        float removeAmount = moles * (1f - MathF.Pow(1f - 0.25f, secondsPerTick));
                        removeAmount = Math.Min(removeAmount, moles);
                        fusionRemovedMix.AdjustMoles(gas, removeAmount);
                        core.InternalFusion.SetMoles(gas, moles - removeAmount);
                    }
                }

                fusionRemovedMix.Temperature = core.InternalFusion.Temperature;
            }

            // Set temperatures and merge moderator mix
            if (moderatorRemovedMix.TotalMoles > 0f)
                _atmosSystem.Merge(wastePipe.Air, moderatorRemovedMix);

            // Set temperatures and merge fusion mix
            if (fusionRemovedMix.TotalMoles > 0f)
                _atmosSystem.Merge(wastePipe.Air, fusionRemovedMix);
        }

        public void ProcessInternalCooling(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick)
        {
            if (core.ModeratorInternal != null && core.InternalFusion != null && core.ModeratorInternal.TotalMoles > 0f && core.InternalFusion.TotalMoles > 0f)
            {
                // Modifies the moderator_internal temperature based on energy conduction and also the fusion by the same amount
                float fusionTemperatureDelta = core.InternalFusion.Temperature - core.ModeratorInternal.Temperature;
                float fusionHeatAmount = (1f - MathF.Pow(1f - HypertorusFusionReactor.MetallicVoidConductivity, secondsPerTick)) * fusionTemperatureDelta *
                                        (_atmosSystem.GetHeatCapacity(core.InternalFusion, true) * _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true) /
                                        (_atmosSystem.GetHeatCapacity(core.InternalFusion, true) + _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true)));
                core.InternalFusion.Temperature = Math.Max(core.InternalFusion.Temperature - fusionHeatAmount / _atmosSystem.GetHeatCapacity(core.InternalFusion, true), Atmospherics.TCMB);
                core.ModeratorInternal.Temperature = Math.Max(core.ModeratorInternal.Temperature + fusionHeatAmount / _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true), Atmospherics.TCMB);
            }

            if (!_nodeContainer.TryGetNode(coreUid, "pipe", out PipeNode? corePipe) || corePipe.Air.TotalMoles * 0.05f <= 0.01)
                return;

            var coolingPort = corePipe.Air;
            var coolingRemove = coolingPort.Remove(0.05f * coolingPort.TotalMoles);
            // Cooling of the moderator gases with the cooling loop in and out the core
            if (core.ModeratorInternal != null && core.ModeratorInternal.TotalMoles > 0f)
            {
                float coolantTemperatureDelta = coolingRemove.Temperature - core.ModeratorInternal.Temperature;
                float coolingHeatAmount = (1f - MathF.Pow(1f - HypertorusFusionReactor.HighEfficiencyConductivity, secondsPerTick)) * coolantTemperatureDelta *
                                        (_atmosSystem.GetHeatCapacity(coolingRemove, true) * _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true) /
                                        (_atmosSystem.GetHeatCapacity(coolingRemove, true) + _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true)));
                coolingRemove.Temperature = Math.Max(coolingRemove.Temperature - coolingHeatAmount / _atmosSystem.GetHeatCapacity(coolingRemove, true), Atmospherics.TCMB);
                core.ModeratorInternal.Temperature = Math.Max(core.ModeratorInternal.Temperature + coolingHeatAmount / _atmosSystem.GetHeatCapacity(core.ModeratorInternal, true), Atmospherics.TCMB);
            }
            else if (core.InternalFusion != null && core.InternalFusion.TotalMoles > 0f)
            {
                float coolantTemperatureDelta = coolingRemove.Temperature - core.InternalFusion.Temperature;
                float coolingHeatAmount = (1f - MathF.Pow(1f - HypertorusFusionReactor.MetallicVoidConductivity, secondsPerTick)) * coolantTemperatureDelta *
                                        (_atmosSystem.GetHeatCapacity(coolingRemove, true) * _atmosSystem.GetHeatCapacity(core.InternalFusion, true) /
                                        (_atmosSystem.GetHeatCapacity(coolingRemove, true) + _atmosSystem.GetHeatCapacity(core.InternalFusion, true)));
                coolingRemove.Temperature = Math.Max(coolingRemove.Temperature - coolingHeatAmount / _atmosSystem.GetHeatCapacity(coolingRemove, true), Atmospherics.TCMB);
                core.InternalFusion.Temperature = Math.Max(core.InternalFusion.Temperature + coolingHeatAmount / _atmosSystem.GetHeatCapacity(core.InternalFusion, true), Atmospherics.TCMB);
            }
            _atmosSystem.Merge(coolingPort, coolingRemove);
        }

        public void InjectFromSideComponents(EntityUid coreUid, HFRCoreComponent core, float secondsPerTick, float fuelInputRate, float moderatorInputRate)
        {
            // Check and store the gases from the moderator input in the moderator internal gasmix
            if (core.ModeratorInputUid != null && _nodeContainer.TryGetNode(core.ModeratorInputUid.Value, "pipe", out PipeNode? moderatorPipe) &&
                core.IsModeratorInjecting && moderatorPipe.Air.TotalMoles > 0f)
            {
                if (core.ModeratorInternal != null)
                {
                    var removed = moderatorPipe.Air.Remove(moderatorInputRate * secondsPerTick);
                    _atmosSystem.Merge(core.ModeratorInternal, removed);
                }
            }

            // Check if the fuels are present and move them inside the fuel internal gasmix
            FusionRecipePrototype? recipe = null;
            if (!core.IsFuelInjecting || string.IsNullOrEmpty(core.SelectedRecipeId) || !_prototypeManager.TryIndex<FusionRecipePrototype>(core.SelectedRecipeId, out recipe) || !CheckGasRequirements(core, recipe))
                return;

            if (core.FuelInputUid != null && _nodeContainer.TryGetNode(core.FuelInputUid.Value, "pipe", out PipeNode? fuelPipe) && core.InternalFusion != null)
            {
                var fuelMix = new GasMixture();
                foreach (var gas in recipe.Requirements.Select(gasId => Enum.Parse<Gas>(gasId)))
                {
                    float moles = fuelPipe.Air.GetMoles(gas);
                    if (moles > 0f)
                    {
                        float removeAmount = Math.Min(moles, fuelInputRate * secondsPerTick / recipe.Requirements.Count);
                        fuelPipe.Air.SetMoles(gas, moles - removeAmount);
                        fuelMix.AdjustMoles(gas, removeAmount);
                    }
                }

                if (fuelMix.TotalMoles > 0f)
                {
                    fuelMix.Temperature = fuelPipe.Air.Temperature;
                    _atmosSystem.Merge(core.InternalFusion, fuelMix);
                }
            }
        }

        /// <summary>
        /// Shoot lightning bolts based on provided parameters.
        /// </summary>
        private void HFRZap(EntityUid uid, float zapRange, int zapNumber, int zapPower)
        {
            var lightningPrototypes = new[] { "SupermatterLightning", "SupermatterLightningCharged", "SupermatterLightningSupercharged" };

            zapPower = Math.Clamp(zapPower, 0, lightningPrototypes.Length - 1);

            _lightning.ShootRandomLightnings(
                uid,
                zapRange,
                zapNumber,
                lightningPrototypes[zapPower],
                arcDepth: 0,
                triggerLightningEvents: true,
                hitCoordsChance: 0.5f
            );
        }
    }
}