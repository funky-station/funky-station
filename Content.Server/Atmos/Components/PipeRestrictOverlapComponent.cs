// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Atmos.Components;

/// <summary>
/// This is used for restricting anchoring pipes so that they do not overlap.
/// </summary>
[RegisterComponent, Access(typeof(PipeRestrictOverlapSystem))]
public sealed partial class PipeRestrictOverlapComponent : Component;
