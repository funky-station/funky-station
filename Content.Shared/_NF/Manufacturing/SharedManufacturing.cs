// SPDX-FileCopyrightText: 2025 Whatstone <166147148+whatston3@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._NF.Manufacturing;

[Serializable, NetSerializable]
public enum EntitySpawnMaterialVisuals : byte
{
    /// <summary>
    /// Whether or not the machine has enough materials to continue processing a unit.
    /// </summary>
    SufficientMaterial
}
