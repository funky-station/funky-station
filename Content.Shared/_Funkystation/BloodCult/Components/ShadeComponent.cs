// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Spooky fella.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadeComponent : Component
{
	/// <summary>
	/// The soulstone that this shade was summoned from. The shade will return here on death.
	/// </summary>
	[DataField]
	public EntityUid? OriginSoulstone = null;
}
