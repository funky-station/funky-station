// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Which visual variant of blood cult dagger this entity is; used when transmuting on the summoning rune.
/// </summary>
[Serializable, NetSerializable]
public enum CultDaggerVariant : byte
{
	Straight,
	Serrated,
	Curved,
}

/// <summary>
/// Marks a blood cult dagger variant for summoning-run upgrades.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class DaggerTypeComponent : Component
{
	[DataField, AutoNetworkedField]
	public CultDaggerVariant Variant;
}
