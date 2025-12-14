// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Medical.IV;

[Serializable, NetSerializable]
public sealed partial class AttachBloodPackDoAfterEvent : SimpleDoAfterEvent;
