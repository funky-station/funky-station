// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Materials.MaterialSilo;

/// <summary>
/// An entity with <see cref="MaterialStorageComponent"/> that interfaces with an <see cref="MaterialSiloComponent"/>.
/// Used for tracking the connected silo.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMaterialSiloSystem))]
public sealed partial class MaterialSiloClientComponent : Component
{
    /// <summary>
    /// The silo that this client pulls materials from.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Silo;
}
