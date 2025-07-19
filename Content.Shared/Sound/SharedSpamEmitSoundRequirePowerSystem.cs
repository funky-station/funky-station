// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Sound;

public abstract partial class SharedSpamEmitSoundRequirePowerSystem : EntitySystem
{
    [Dependency] protected readonly SharedEmitSoundSystem EmitSound = default!;
}
