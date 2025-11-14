// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CosmicFinaleComponent : Component
{
    [DataField]
    public FinaleState CurrentState = FinaleState.Unavailable;

    [DataField]
    public bool FinaleDelayStarted = false;

    [DataField]
    public bool FinaleActive = false;

    [DataField]
    public bool Occupied = false;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan FinaleTimer = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan BufferTimer = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CultistsCheckTimer = default!;

    [DataField, AutoNetworkedField]
    public TimeSpan BufferRemainingTime = TimeSpan.FromSeconds(360);

    [DataField, AutoNetworkedField]
    public TimeSpan FinaleRemainingTime = TimeSpan.FromSeconds(126);

    [DataField, AutoNetworkedField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public TimeSpan BufferSpeedup = TimeSpan.FromSeconds(40); // Funky. How much to speed up the buffer phase when converting someone.

    [DataField, AutoNetworkedField]
    public TimeSpan BufferSpeedupFaloff = TimeSpan.FromSeconds(5); // Funky. Each time someone is converted during buffer phase, the speedup gets reduced by this value.

    [DataField]
    public SoundSpecifier CancelEventSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");

    [DataField]
    public TimeSpan FinaleSongLength;

    [DataField]
    public TimeSpan SongLength;

    [DataField]
    public SoundSpecifier? SelectedSong;

    [DataField]
    public TimeSpan InteractionTime = TimeSpan.FromSeconds(30);

    [DataField]
    public SoundSpecifier BufferMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/premonition.ogg");

    [DataField]
    public SoundSpecifier FinaleMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/a_new_dawn.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? SongTimer;

    /// <summary>
    /// The degen that people suffer if they don't have mindshields, aren't a chaplain, or aren't cultists while the Finale is Available or Active. This feature is currently disabled.
    /// </summary>
    [DataField]
    public DamageSpecifier FinaleDegen = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2.25},
            { "Cold", 2.25},
            { "Radiation", 2.25},
            { "Asphyxiation", 2.25}
        }
    };
}

[Serializable]
public enum FinaleState : byte
{
    Unavailable,
    ReadyBuffer,
    ReadyFinale,
    ActiveBuffer,
    ActiveFinale,
    Victory,
}
