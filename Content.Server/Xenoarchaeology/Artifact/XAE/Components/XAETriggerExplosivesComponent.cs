// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Explosion.Components.OnTrigger;

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// Activates 'trigger' for <see cref="ExplodeOnTriggerComponent"/>.
/// </summary>
[RegisterComponent, Access(typeof(XAETriggerExplosivesSystem))]
public sealed partial class XAETriggerExplosivesComponent : Component;
