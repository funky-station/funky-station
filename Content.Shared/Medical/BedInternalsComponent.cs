// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Medical;

[Serializable, NetSerializable]
public enum BedInternalsVisuals
{
    TankVisual
}

[Serializable, NetSerializable]
public enum BedTankVisual
{
    None,
    Nitrogen,
    Oxygen,
    Generic,
    Plasma,
    Nitrous
}

[RegisterComponent, NetworkedComponent]
public sealed partial class BedInternalsComponent : Component
{
    [DataField("slot", required: true)]
    public string GasSlot = default!;

    [DataField("maskPrototype")]
    public string MaskPrototype = "ClothingMaskBreathMedical";

    public bool Enabled;
    public EntityUid? CachedTank;

    public Dictionary<EntityUid, EntityUid> TempMasks = new();
    public Dictionary<EntityUid, EntityUid> StoredMasks = new();
}
