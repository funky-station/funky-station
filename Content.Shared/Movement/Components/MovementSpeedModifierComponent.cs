// SPDX-FileCopyrightText: 2020 4dplanner <3combined@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 KISS <59531932+YuriyKiss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components
{
    /// <summary>
    /// Applies basic movement speed and movement modifiers for an entity.
    /// If this is not present on the entity then they will use defaults for movement.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(MovementSpeedModifierSystem))]
    public sealed partial class MovementSpeedModifierComponent : Component
    {
        #region defaults

        // weightless
        public const float DefaultWeightlessFriction = 1f;
        public const float DefaultWeightlessModifier = 0.7f;
        public const float DefaultWeightlessAcceleration = 1f;

        // friction
        public const float DefaultAcceleration = 20f;
        public const float DefaultFriction = 2.5f;
        public const float DefaultFrictionNoInput = 2.5f;
        public const float DefaultMinimumFrictionSpeed = 0.005f;

        // movement
        public const float DefaultBaseWalkSpeed = 2.5f;
        public const float DefaultBaseSprintSpeed = 4.5f;

        #endregion

        #region base values

        /// <summary>
        /// These base values should be defined in yaml and rarely if ever modified directly.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float BaseWalkSpeed = DefaultBaseWalkSpeed;

        [DataField, AutoNetworkedField]
        public float BaseSprintSpeed = DefaultBaseSprintSpeed;

        /// <summary>
        /// The acceleration applied to mobs when moving. If this is ever less than Friction the mob will be slower.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float BaseAcceleration = DefaultAcceleration;

        /// <summary>
        /// The body's base friction modifier that is applied in *all* circumstances.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float BaseFriction = DefaultFriction;

        /// <summary>
        /// Minimum speed a mob has to be moving before applying movement friction.
        /// </summary>
        [DataField]
        public float MinimumFrictionSpeed = DefaultMinimumFrictionSpeed;

        #endregion

        #region calculated values

        [ViewVariables]
        public float CurrentWalkSpeed => WalkSpeedModifier * BaseWalkSpeed;
        [ViewVariables]
        public float CurrentSprintSpeed => SprintSpeedModifier * BaseSprintSpeed;

        /// <summary>
        /// The acceleration applied to mobs when moving. If this is ever less than Friction the mob will be slower.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float Acceleration;

        /// <summary>
        /// Modifier to the negative velocity applied for friction.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float Friction;

        /// <summary>
        /// The negative velocity applied for friction.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float FrictionNoInput;

        #endregion

        #region movement modifiers

        [AutoNetworkedField, ViewVariables]
        public float WalkSpeedModifier = 1.0f;

        [AutoNetworkedField, ViewVariables]
        public float SprintSpeedModifier = 1.0f;

        #endregion

        #region Weightless

        /// <summary>
        /// These base values should be defined in yaml and rarely if ever modified directly.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float BaseWeightlessFriction = DefaultWeightlessFriction;

        [AutoNetworkedField, DataField]
        public float BaseWeightlessModifier = DefaultWeightlessModifier;

        [AutoNetworkedField, DataField]
        public float BaseWeightlessAcceleration = DefaultWeightlessAcceleration;

        /*
         * Final values
         */

        [ViewVariables]
        public float WeightlessWalkSpeed => WeightlessModifier * BaseWalkSpeed;
        [ViewVariables]
        public float WeightlessSprintSpeed => WeightlessModifier * BaseSprintSpeed;

        /// <summary>
        /// The acceleration applied to mobs when moving and weightless.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float WeightlessAcceleration;

        /// <summary>
        /// The movement speed modifier applied to a mob's total input velocity when weightless.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float WeightlessModifier;

        /// <summary>
        /// The negative velocity applied for friction when weightless and providing inputs.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float WeightlessFriction;

        /// <summary>
        /// The negative velocity applied for friction when weightless and not providing inputs.
        /// </summary>
        [AutoNetworkedField, DataField]
        public float WeightlessFrictionNoInput;

        /// <summary>
        /// The negative velocity applied for friction when weightless and not standing on a grid or mapgrid
        /// </summary>
        [AutoNetworkedField, DataField]
        public float? OffGridFriction;

        #endregion
    }
}
