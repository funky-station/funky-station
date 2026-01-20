// SPDX-FileCopyrightText: 2025 jhrushbe <capnmerry@gmail.com>
//
// SPDX-License-Identifier: CC-BY-NC-SA-3.0

using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NuclearReactorMonitorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public NetEntity? reactor;

    [DataField]
    public ProtoId<SinkPortPrototype> LinkingPort = "NuclearReactorDataReceiver";
}