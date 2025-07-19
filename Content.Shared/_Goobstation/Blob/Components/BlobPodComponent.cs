// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobPodComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsZombifying = false;

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ZombifiedEntityUid = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifyDelay")]
    public float ZombifyDelay = 5.00f;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Core = null;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Factory = null;

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifySoundPath")]
    public SoundSpecifier ZombifySoundPath = new SoundPathSpecifier("/Audio/Effects/Fluids/blood1.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifyFinishSoundPath")]
    public SoundSpecifier ZombifyFinishSoundPath = new SoundPathSpecifier("/Audio/Effects/gib1.ogg");

    public Entity<AudioComponent>? ZombifyStingStream;
    public EntityUid? ZombifyTarget;
}
