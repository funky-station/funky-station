// SPDX-FileCopyrightText: 2025 mycobiota <154991750+mycobiota@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Tools.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CheapLighterComponent : Component
{
    /// <summary>
    /// An additional sound the lighter should play when switched on, which can be interrupted when it's closed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundActivate;

    /// <summary>
    /// The chance the lighter will fail to light, between 0.0 and 1.0.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float FailChance = 0f;

    /// <summary>
    /// The sound the lighter will play when it fails to light.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundFail;
}
