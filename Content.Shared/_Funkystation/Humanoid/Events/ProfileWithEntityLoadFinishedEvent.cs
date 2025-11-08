// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Preferences;

namespace Content.Shared._Funkystation.Humanoid.Events;

public record struct ProfileWithEntityLoadFinishedEvent(EntityUid Uid, HumanoidCharacterProfile Profile);