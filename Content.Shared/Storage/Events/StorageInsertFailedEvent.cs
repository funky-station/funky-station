// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.Components;

namespace Content.Shared.Storage.Events;

[ByRefEvent]
public record struct StorageInsertFailedEvent(Entity<StorageComponent?> Storage, Entity<HandsComponent?> Player);
