// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat;

/// <summary>
///     Makes the entity speak when triggered. If the item has UseDelay component, the system will respect that cooldown.
/// </summary>
[RegisterComponent]
public sealed partial class SpeakOnTriggerComponent : Component
{
    /// <summary>
    ///     The identifier for the dataset prototype containing messages to be spoken by this entity.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<LocalizedDatasetPrototype> Pack = string.Empty;
}
