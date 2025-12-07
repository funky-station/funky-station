// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Lock;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;

namespace Content.Server._EinsteinEngines.Silicon.EncryptionHolderRequiresLock;

public sealed class EncryptionHolderRequiresLockSystem : EntitySystem

{
    [Dependency] private readonly EncryptionKeySystem _encryptionKeySystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EncryptionHolderRequiresLockComponent, LockToggledEvent>(LockToggled);

    }
    private void LockToggled(EntityUid uid, EncryptionHolderRequiresLockComponent component, LockToggledEvent args)
    {
        if (!TryComp<LockComponent>(uid, out var lockComp)
            || !TryComp<EncryptionKeyHolderComponent>(uid, out var keyHolder))
            return;

        keyHolder.KeysUnlocked = !lockComp.Locked;
        _encryptionKeySystem.UpdateChannels(uid, keyHolder);
    }
}
