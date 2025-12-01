// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Leo <lzimann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Contains all player characters and the index of the currently selected character.
    ///     Serialized both over the network and to disk.
    /// </summary>
    [Serializable]
    [NetSerializable]
    public sealed class PlayerPreferences
    {
        private Dictionary<int, ICharacterProfile> _characters;

        public PlayerPreferences(IEnumerable<KeyValuePair<int, ICharacterProfile>> characters, int selectedCharacterIndex, Color adminOOCColor, List<ProtoId<ConstructionPrototype>> constructionFavorites)
        {
            _characters = new Dictionary<int, ICharacterProfile>(characters);
            SelectedCharacterIndex = selectedCharacterIndex;
            AdminOOCColor = adminOOCColor;
            ConstructionFavorites = constructionFavorites;
        }

        /// <summary>
        ///     All player characters.
        /// </summary>
        public IReadOnlyDictionary<int, ICharacterProfile> Characters => _characters;

        public ICharacterProfile GetProfile(int index)
        {
            return _characters[index];
        }

        /// <summary>
        ///     Index of the currently selected character.
        /// </summary>
        public int SelectedCharacterIndex { get; }

        /// <summary>
        ///     The currently selected character.
        /// </summary>
        public ICharacterProfile SelectedCharacter => Characters[SelectedCharacterIndex];

        public Color AdminOOCColor { get; set; }

        /// <summary>
        ///    List of favorite items in the construction menu.
        /// </summary>
        public List<ProtoId<ConstructionPrototype>> ConstructionFavorites { get; set; } = [];

        public int IndexOfCharacter(ICharacterProfile profile)
        {
            return _characters.FirstOrNull(p => p.Value == profile)?.Key ?? -1;
        }

        public bool TryIndexOfCharacter(ICharacterProfile profile, out int index)
        {
            return (index = IndexOfCharacter(profile)) != -1;
        }
        /// <summary>
        /// Get job priorities, but filtered by the presence of enabled characters asking for that job
        /// </summary>
        public Dictionary<ProtoId<JobPrototype>, JobPriority> JobPrioritiesFiltered()
        {
            var allCharacterJobs = new HashSet<ProtoId<JobPrototype>>();
            var allJobPriorities = new Dictionary<ProtoId<JobPrototype>, JobPriority>();

            // Collect all jobs from all characters and aggregate their priorities
            foreach (var profile in Characters.Values)
            {
                if (profile is not HumanoidCharacterProfile humanoid)
                    continue;

                // Add all jobs this character has preferences for
                foreach (var (job, priority) in humanoid.JobPriorities)
                {
                    allCharacterJobs.Add(job);

                    // Keep the highest priority for each job across all characters
                    if (!allJobPriorities.ContainsKey(job) || priority > allJobPriorities[job])
                    {
                        allJobPriorities[job] = priority;
                    }
                }
            }

            // Filter to only return jobs that enabled characters have requested
            var filteredPlayerJobs = new Dictionary<ProtoId<JobPrototype>, JobPriority>();
            foreach (var (job, priority) in allJobPriorities)
            {
                if (allCharacterJobs.Contains(job))
                {
                    filteredPlayerJobs.Add(job, priority);
                }
            }

            return filteredPlayerJobs;
        }
    }
}
