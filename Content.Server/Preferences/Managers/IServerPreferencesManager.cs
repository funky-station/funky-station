// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Quantum-cross <7065792+Quantum-cross@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Preferences.Managers
{
    public interface IServerPreferencesManager
    {
        void Init();

        Task LoadData(ICommonSession session, CancellationToken cancel);
        void FinishLoad(ICommonSession session);
        void OnClientDisconnected(ICommonSession session);

        bool TryGetCachedPreferences(NetUserId userId, [NotNullWhen(true)] out PlayerPreferences? playerPreferences);
        PlayerPreferences GetPreferences(NetUserId userId);
        PlayerPreferences? GetPreferencesOrNull(NetUserId? userId);
        bool HavePreferencesLoaded(ICommonSession session);

        Task SetProfile(NetUserId userId, int slot, ICharacterProfile profile);

        /// <summary>
        /// Save a player's job priorities to their player profile.
        /// </summary>
        Task SetJobPriorities(NetUserId userId, Dictionary<ProtoId<JobPrototype>, JobPriority> jobPriorities);

        /// <summary>
        /// Delete the character profile in the given slot from a player's profile
        /// </summary>
        Task DeleteProfile(NetUserId userId, int slot);
    }

    public sealed class PlayerJobPriorityChangedEvent : EntityEventArgs
    {
        public readonly ICommonSession Session;
        public readonly Dictionary<ProtoId<JobPrototype>, JobPriority> OldPriorities;
        public readonly Dictionary<ProtoId<JobPrototype>, JobPriority> NewPriorities;

        public PlayerJobPriorityChangedEvent(ICommonSession session,
        Dictionary<ProtoId<JobPrototype>, JobPriority> oldPriorities,
        Dictionary<ProtoId<JobPrototype>, JobPriority> newPriorities)
        {
            Session = session;
            OldPriorities = oldPriorities;
            NewPriorities = newPriorities;
        }
    }
}
