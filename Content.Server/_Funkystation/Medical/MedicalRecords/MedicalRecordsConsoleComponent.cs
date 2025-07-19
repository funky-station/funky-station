// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.StationRecords;

namespace Content.Server._Funkystation.Medical.MedicalRecords;

[RegisterComponent]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public uint? SelectedIndex { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public StationRecordsFilter? Filter;
}
