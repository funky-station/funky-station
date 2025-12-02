// SPDX-FileCopyrightText: 2025 jhrushbe <capnmerry@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Examine;
using Content.Client.NodeContainer;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    private static readonly EntProtoId ArrowPrototype = "ReactorFlowArrow";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorComponent, ClientExaminedEvent>(ReactorExamined);
    }

    private void ReactorExamined(EntityUid uid, NuclearReactorComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));
    }
}