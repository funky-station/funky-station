// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Thaven.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedThavenMoodSystem))]
public sealed partial class ThavenMoodsComponent : Component
{
    /// <summary>
    /// Whether to include SharedMoods that all thaven have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FollowsSharedMoods = true;

    /// <summary>
    /// The non-shared moods that are active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ThavenMood> Moods = new();

    /// <summary>
    /// Whether to allow emagging to add a random wildcard mood.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanBeEmagged = true;

    /// <summary>
    /// Notification sound played if your moods change.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? MoodsChangedSound = new SoundPathSpecifier("/Audio/_Impstation/Thaven/moods_changed.ogg");

    [DataField(serverOnly: true)]
    public EntityUid? Action;
}

public sealed partial class ToggleMoodsScreenEvent : InstantActionEvent;

[NetSerializable, Serializable]
public enum ThavenMoodsUiKey : byte
{
    Key
}

/// <summary>
/// BUI state to tell the client what the shared moods are.
/// </summary>
[Serializable, NetSerializable]
public sealed class ThavenMoodsBuiState(List<ThavenMood> sharedMoods) : BoundUserInterfaceState
{
    public readonly List<ThavenMood> SharedMoods = sharedMoods;
}
