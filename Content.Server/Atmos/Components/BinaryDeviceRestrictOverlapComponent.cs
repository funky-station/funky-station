// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Atmos.Components;

/// <summary>
/// This is used for restricting anchoring devices so that they do not overlap.
/// </summary>
[RegisterComponent, Access(typeof(BinaryDeviceRestrictOverlapSystem))]
public sealed partial class BinaryDeviceRestrictOverlapComponent : Component;
