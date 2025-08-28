// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 pre-commit-ci[bot] <66853113+pre-commit-ci[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._DV.Silicon.IPC;

[RegisterComponent]
public sealed partial class SnoutHelmetComponent : Component
{
    [DataField]
    public bool EnableAlternateHelmet;

    [DataField(readOnly: true)]
    public string? ReplacementRace;
}
