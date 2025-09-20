// SPDX-FileCopyrightText: 2025 mnva <218184747+mnva0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.Medical;

[RegisterComponent]
public sealed partial class BodyScannerComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> OperatingTablePort = "OperatingTableReceiver";

    [DataField, AutoNetworkedField]
    public EntityUid? OperatingTable;
}
